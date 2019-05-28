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

namespace SalaryExplorer.Controllers
{
  [Route("api/Values")]
  public class ValuesController : Controller
{
    Dal _dal;

    public ValuesController()
    {
      _dal = new Dal();
    }

    // GET api/values
    [HttpGet("{tableName}")]
    public async Task<IEnumerable<Column>> GetColumnNames([FromRoute] string tableName)
    {
      string procName = "GetTablesAndColumns";
      var procParams = new Dictionary<string, string>();
      procParams["@tableName"] = tableName;

      List<Column> columns = await _dal.GetData<Column>(procParams, procName, ConnectionSettings.ExploreConnString);

      columns.ForEach(x => x.PropsToLower());

      return columns;
    }

    //POST api/values
   [HttpPost]
    public async Task<List<JObject>> Post([FromBody] object record)
    {
      try
      {
        string query = QueryHelpers.BuildQuery((JObject)record);

        List<JObject> records = _dal.GetData(query, ConnectionSettings.ExploreConnString, (JObject)record);

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
        return StatusCode(500, "Internal server error");
      }
    }

    [HttpPost, DisableRequestSizeLimit]
    [Route("[action]")]
    public async Task<IActionResult> CreateTable([FromBody] TableFile tableFile)
    {
      try
      {
        string procName = "InsertDataTable";

        Dictionary<string, string> param = new Dictionary<string, string>();
        param["@TableName"] = tableFile.GetTableName();
        param["@Description"] = tableFile.Description == null ? "" : tableFile.Description;
        int id = await _dal.NonQuery(param, procName, ConnectionSettings.InsertConnString);

        for (int i = 0; i < tableFile.Columns.Count; i++)
        {
          string insertColumnprocName = "InsertTableColumn";
          Dictionary<string, string> columnParam = new Dictionary<string, string>();
          columnParam["@ColumnName"] = tableFile.Columns[i].ColumnName;
          columnParam["@Visible"] = "1";
          columnParam["@DisplayName"] = tableFile.Columns[i].ColumnName;
          columnParam["@DataTableId"] = id.ToString();
          columnParam["@Type"] = tableFile.Columns[i].Type;
          await _dal.NonQuery(columnParam, insertColumnprocName, ConnectionSettings.InsertConnString);
        }

        var folderName = "TableFiles";
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
        var fullPath = Path.Combine(pathToSave, tableFile.FileName);

        string statement = tableFile.CreateTableStatement(fullPath, tableFile);

        await _dal.NonQueryByStatement(statement, ConnectionSettings.InsertConnString);

        string insertStatement = tableFile.GetInsertStatement(fullPath);

        await _dal.NonQueryByStatement(insertStatement, ConnectionSettings.InsertConnString);

        return Ok();
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Internal server error");
      }
    }

  }
}
