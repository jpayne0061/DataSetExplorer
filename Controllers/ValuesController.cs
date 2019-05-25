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

namespace SalaryExplorer.Controllers
{
[Route("api/[controller]")]
public class ValuesController : Controller
{
    Dal _dal;

    public ValuesController()
    {
      _dal = new Dal();
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/values
    [HttpPost]
    public async Task<List<RecordInput>> Post([FromBody] RecordInput record)
    {
      try
      {
        string query = QueryHelpers.BuildQuery(record);

        List<RecordInput> records = await _dal.GetData<RecordInput>(query, ConnectionSettings.SalaryConnString);

        return records;
      }
      catch (Exception ex)
      {
        //todo logging
        throw;
      }
    }

}
}
