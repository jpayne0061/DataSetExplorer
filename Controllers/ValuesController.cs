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

        Query query = QueryHelpers.BuildQuery(jobj, columns);

        Dictionary<string, string> colNameToPsedonym = columns.ToDictionary(x => x.ColumnName, x => x.Pseudonym);

        List<JObject> records = await _dal.GetDataWithParameters(query, ConnectionSettings.ExploreConnString, jobj, colNameToPsedonym);

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
        Guid guid = Guid.NewGuid();

        string tableGuid = guid.ToString();

        //save meta data about data set
        int insertedId = await tableFile.InsertDataTable(_dal, tableGuid);

        //save meta data about columns
        tableFile.InsertColumns(insertedId, _dal);

        //create table and insert rows into it
        try
        {
          var folderName = "TableFiles";
          string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
          string fullPath = Path.Combine(pathToSave, tableFile.FileName);

          tableFile.CreateTableAndLoadData(tableGuid, pathToSave, fullPath, _dal, _signalHubContext);

          return Ok();
        }
        catch (Exception ex)
        {
          tableFile.RollBackInserts(insertedId, _dal, tableGuid);

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
