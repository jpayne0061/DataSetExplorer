using Newtonsoft.Json.Linq;
using SalaryExplorer.Data.Interfaces;
using SalaryExplorer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SalaryExplorer.Data.Get
{
  public class Dal
  {
    public async Task<List<T>> GetData<T>(string query, string connStr) where T : new()
    {
      try
      {
        var data = new List<T>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(query, conn))
        {
          await conn.OpenAsync();

          var rdr = await command.ExecuteReaderAsync();

          while (await rdr.ReadAsync())
          {
            T obj = new T();

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
              object val = rdr[pi.Name];
              pi.SetValue(obj, val.ToString());

            }
            data.Add(obj);
          }
        }
        return data;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public async Task<List<T>> GetData<T>(Dictionary<string, string> param, string procName, string connStr) where T : new()
    {
      try
      {
        var data = new List<T>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(procName, conn))
        {
          command.CommandType = CommandType.StoredProcedure;

          foreach(KeyValuePair<string, string> kvp in param)
          {
            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
          }

          await conn.OpenAsync();

          var rdr = await command.ExecuteReaderAsync();

          while (await rdr.ReadAsync())
          {
            T obj = new T();

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
              object val = rdr[pi.Name];
              pi.SetValue(obj, val.ToString());

            }
            data.Add(obj);
          }
        }
        return data;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public async Task<int> NonQuery(Dictionary<string, string> param, string procName, string connStr)
    {
      try
      {
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(procName, conn))
        {
          command.CommandType = CommandType.StoredProcedure;

          foreach (KeyValuePair<string, string> kvp in param)
          {
            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
          }

          await conn.OpenAsync();

          int id = Convert.ToInt32(command.ExecuteScalar());
          return id;
         }
      }
      catch (Exception ex)
      {
        throw;
      }
    }


    public async Task NonQueryByStatement(string statement, string connStr)
    {
      try
      {
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(statement, conn))
        {
          await conn.OpenAsync();

          await command.ExecuteNonQueryAsync();
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public List<JObject> GetData(string query, string connStr, JObject record)
    {
      try
      {
        var data = new List<JObject>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(query, conn))
        {
          conn.Open();

          var rdr = command.ExecuteReader();

          while (rdr.Read())
          {
            JObject obj = new JObject();

            List<JProperty> properties = record.Properties().ToList();

            foreach (JProperty pi in properties)
            {
              string val = rdr[pi.Name].ToString();
              obj[pi.Name.FirstCharToLower()] = val;
            }
            data.Add(obj);
          }
        }
        return data;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public async Task<List<object>> GetDataObjects(string query, string connStr)
    {
      try
      {
        var data = new List<object>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(query, conn))
        {
          conn.Open();

          var rdr = await command.ExecuteReaderAsync();

          while (await rdr.ReadAsync())
          {
            object obj = new object();

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
              object val = rdr[pi.Name];
              pi.SetValue(obj, val);

            }
            data.Add(obj);
          }
        }
        return data;
      }
      catch (Exception ex)
      {
        throw;
      }

    }

  }
}
