using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SalaryExplorer.Models;
using System.Reflection;
using SalaryExplorer.Helpers;
using System.Data;
using SalaryExplorer.Data.Get;
using SalaryExplorer.Data.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http.Headers;
using SalaryExplorer.ExtensionMethods;
using SalaryExplorer.Settings;
using Microsoft.AspNetCore.SignalR;

namespace SalaryExplorer.Controllers
{
  [Route("api/Values")]
  public class ValuesController : Controller
{
    Dal _dal;
    private readonly IHubContext<SignalRHub> _signalHubContext;

    public ValuesController(IHubContext<SignalRHub> signalHubContext)
    {
      _signalHubContext = signalHubContext;
      _dal = new Dal();
    }

    // GET api/values
    [HttpGet("{tableGuid}")]
    public async Task<IEnumerable<Column>> GetColumnNames([FromRoute] string tableGuid)
    {
      string procName = "GetTablesAndColumns";
      var procParams = new Dictionary<string, string>();
      procParams["@tableGuid"] = tableGuid;

      List<Column> columns = await _dal.GetData<Column>(procParams, procName, ConnectionSettings.ExploreConnString);

      columns.ForEach(x => x.PropsToLower());

      return columns;
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IEnumerable<TableFile>> GetTables()
    {
      string procName = "GetTables";
      var procParams = new Dictionary<string, string>();

      List<TableFile> tables = await _dal.GetTableFiles(ConnectionSettings.ExploreConnString);

      //tables.ForEach(x => x.TableGuidPropsToLower());

      return tables;
    }


    //POST api/values
    [HttpPost]
    public async Task<List<JObject>> Post([FromBody] object record)
    {
      try
      {
        JObject jobj = (JObject)record;

        string procName = "GetTablesAndColumns";
        var procParams = new Dictionary<string, string>();
        procParams["@tableGuid"] = (string)jobj.SelectToken(Configurations.ProtectedPropertyTableName);

        List<Column> columns = await _dal.GetData<Column>(procParams, procName, ConnectionSettings.ExploreConnString);

        string query = QueryHelpers.BuildQuery(jobj, columns);

        Dictionary<string, string> colNameToPsedonym = columns.ToDictionary(x => x.ColumnName, x => x.Pseudonym);

        List<JObject> records = _dal.GetData(query, ConnectionSettings.ExploreConnString, jobj, colNameToPsedonym);

        return records;
      }
      catch (Exception ex)
      {
        //todo logging
        throw;
      }
    }

    [HttpPost, DisableRequestSizeLimit]
    [Route("[action]")]
    public IActionResult Upload()
    {
      try
      {
        var file = Request.Form.Files[0];
        var folderName = "TableFiles";

        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        if (file.Length > 0)
        {
          var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
          var fullPath = Path.Combine(pathToSave, fileName);
          var dbPath = Path.Combine(folderName, fileName);

          using (var stream = new FileStream(fullPath, FileMode.Create))
          {
            file.CopyTo(stream);
          }

          TableFile tableFile = new TableFile();
          tableFile.FileName = fileName;
          tableFile.SetColumns(fullPath);

          return Ok(tableFile);

          //return Ok(new { dbPath });
        }
        else
        {
          return BadRequest();
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpPost, DisableRequestSizeLimit]
    [Route("[action]")]
    public async Task<IActionResult> CreateTable([FromBody] TableFile tableFile)
    {
      try
      {
        tableFile.Columns.ForEach(col =>
          col.ColumnName = col.ColumnName.Replace("\"", "").Replace("'", "").Replace("'", "").ToLower());

        Guid guid = Guid.NewGuid();

        string tableGuid = guid.ToString();

        string procName = "InsertDataTable";

        Dictionary<string, string> param = new Dictionary<string, string>();
        param["@TableName"] = tableFile.GetTableName();
        param["@Description"] = tableFile.Description == null ? "" : tableFile.Description;
        param["@Guid"] = tableGuid;
        param["@DataSetTitle"] = tableFile.DataSetTitle;
        int id = await _dal.NonQuery(param, procName, ConnectionSettings.InsertConnString);

        for (int i = 0; i < tableFile.Columns.Count; i++)
        {
          string pseudonym = Guid.NewGuid().ToString();
          tableFile.Columns[i].Pseudonym = pseudonym;
          string insertColumnprocName = "InsertTableColumn";
          Dictionary<string, string> columnParam = new Dictionary<string, string>();
          columnParam["@ColumnName"] = tableFile.Columns[i].ColumnName;
          columnParam["@Visible"] = tableFile.Columns[i].Visible == null ? "0" : tableFile.Columns[i].Visible;
          columnParam["@DisplayName"] = tableFile.Columns[i].DisplayName == null ? tableFile.Columns[i].ColumnName : tableFile.Columns[i].DisplayName.Replace("\"", "");
          columnParam["@DataTableId"] = id.ToString();
          columnParam["@Type"] = tableFile.Columns[i].Type ?? "varchar(500)";
          columnParam["@Pseudonym"] = pseudonym;
          await _dal.NonQuery(columnParam, insertColumnprocName, ConnectionSettings.InsertConnString);
        }

        bool tableCreated = false;

        try
        {
          var folderName = "TableFiles";
          var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
          var fullPath = Path.Combine(pathToSave, tableFile.FileName);

          string createTableStatement = tableFile.CreateTableStatement(fullPath, tableFile, tableGuid);

          try
          {
            await _dal.NonQueryByStatement(createTableStatement, ConnectionSettings.InsertConnString);
            tableCreated = true;
          }
          catch (Exception ex)
          {
            await _signalHubContext.Clients.All.SendAsync("data update", "table creation failed: " + ex.Message);
            throw;
          }

          await _signalHubContext.Clients.All.SendAsync("data update", "table created");

          SqlConnection conn = new SqlConnection(ConnectionSettings.InsertConnString);

          InsertObject insertObject;

          try
          {
            insertObject = tableFile.GetInsertStatement(fullPath, tableGuid);
          }
          catch(Exception ex)
          {
            await _signalHubContext.Clients.All.SendAsync("data update", "DML statements failed: " + ex.Message);
            throw;
          }

          await _signalHubContext.Clients.All.SendAsync("rows found", insertObject.RowCount);

          string[] splitStatements = insertObject.InsertStatements.Split(';');

          int maxRows = (int)((decimal)2000 / tableFile.Columns.Count);

          int numStatements = splitStatements.Length - 1;
          int numToSkip = 0;

          int numToProcess = Math.Min(3000, maxRows);

          while (numStatements > 0)
          {
            string insert = string.Join("", splitStatements.Skip(numToSkip).Take(numToProcess));
            await _dal.ExecuteInsertObject(tableFile, insert, insertObject.ParameterMap, ConnectionSettings.InsertConnString, numToProcess, numToSkip, splitStatements.Length - 1);
            await _signalHubContext.Clients.All.SendAsync("row update", numToSkip);
            numToSkip += maxRows;
            numStatements -= maxRows;
          }

          await _signalHubContext.Clients.All.SendAsync("complete", insertObject.RowCount);

          return Ok();
        }
        catch (Exception ex)
        {
          string deleteColumns = "delete from TableColumns where DataTableId = " + id.ToString();
          string deleteDataTable = "delete from DataTable where DataTableId = " + id.ToString();
          string dropTable = "drop table " + "[" + tableGuid + "]";

          if (tableCreated)
          {
            await _dal.NonQueryByStatement(dropTable, ConnectionSettings.SaConnString);
          }

          await _dal.NonQueryByStatement(deleteColumns, ConnectionSettings.SaConnString);
          await _dal.NonQueryByStatement(deleteDataTable, ConnectionSettings.SaConnString);

          return StatusCode(500, ex.Message);
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

  }
}
