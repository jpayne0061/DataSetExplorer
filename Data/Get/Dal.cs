using SalaryExplorer.Data.Interfaces;
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

    public List<dynamic> GetData(string query, string connStr)
    {
      try
      {
        var data = new List<dynamic>();
        using (var conn = new SqlConnection(connStr))

        using (var command = new SqlCommand(query, conn))
        {
          conn.Open();

          var rdr = command.ExecuteReader();

          while (rdr.Read())
          {
            dynamic obj = new System.Dynamic.ExpandoObject();

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
