using Newtonsoft.Json.Linq;
using SalaryExplorer.Data.Interfaces;
using SalaryExplorer.ExtensionMethods;
using SalaryExplorer.Models;
using SalaryExplorer.Settings;
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

    public async Task<List<JObject>> GetDataWithParameters(Query query, string connStr, JObject record, Dictionary<string, string> colNameToPsedonym)
    {
      try
      {
        var data = new List<JObject>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(query.Body, conn))
        {
          foreach(KeyValuePair<string, string> kvp in query.Parameters)
          {
            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
          }

          await conn.OpenAsync();

          var rdr = await command.ExecuteReaderAsync();

          while (await rdr.ReadAsync())
          {
            JObject obj = new JObject();

            List<JProperty> properties = record.Properties().ToList();

            foreach (JProperty pi in properties)
            {
              if (pi.Name == Configurations.ProtectedPropertyTableName)
              {
                continue;
              }

              string val = rdr[colNameToPsedonym[pi.Name.ToLower()]].ToString();
              obj[pi.Name.ToLower()] = val;
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

    public async Task<List<TableFile>> GetTableFiles(string connStr)
    {
      try
      {
        var data = new List<TableFile>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand("GetTables", conn))
        {
          command.CommandType = CommandType.StoredProcedure;

          await conn.OpenAsync();

          var rdr = await command.ExecuteReaderAsync();

          while (await rdr.ReadAsync())
          {
            TableFile tb = new TableFile();

            tb.TableName = Convert.ToString(rdr["DataSetTitle"]);
            tb.TableGuid = Convert.ToString(rdr["TableGuid"]);
            tb.DataSetTitle = Convert.ToString(rdr["DataSetTitle"]);
            tb.Description = Convert.ToString(rdr["Description"]);

            data.Add(tb);
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

    public async Task ExecuteInsertObject(TableFile tableFile, string insertStatement, Dictionary<string, string> ParameterMap, string connStr, int numStatements, int start, int total)
    {
      SqlCommand command;

      try
      {
        using (var conn = new SqlConnection(connStr))
        {
          using (command = new SqlCommand(insertStatement, conn))
          {
            for (int i = start; i < start + numStatements; i++)
            {
              if(i > total)
              {
                break;
              }
              for (int j = 0; j < tableFile.Columns.Count; j++)
              {
                command.Parameters.AddWithValue("@" + i.ToString() + tableFile.Columns[j].Pseudonym.Replace("-", ""),
                                  ((object)ParameterMap["@" + i.ToString() + tableFile.Columns[j].Pseudonym.Replace("-", "")]) ?? DBNull.Value);
              }
            }

            await conn.OpenAsync();
            await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public List<JObject> GetData(string query, string connStr, JObject record, Dictionary<string, string> colNameToPsedonym)
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
              if (pi.Name == Configurations.ProtectedPropertyTableName)
              {
                continue;
              }

              string val = rdr[colNameToPsedonym[pi.Name.ToLower()]].ToString();
              obj[pi.Name.ToLower()] = val;
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
