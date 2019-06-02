using Newtonsoft.Json.Linq;
using SalaryExplorer.Models;
using SalaryExplorer.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SalaryExplorer.Helpers
{
  public static class QueryHelpers
  {
    public static string LeadingOperator(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return "";
      }

      switch (value[0])
      {
        case '<':
          return "<";
        case '*':
          return "LIKE %";
        case '>':
          return ">";
      }

      return "";
    }


    public static string TrailingOperator(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return "";
      }

      switch (value[value.Length - 1])
      {
        case '*':
          return "%";
      }

      return "";
    }

    public static string GetClause(string propName, string value)
    {
      value = value.Trim();

      string leadingOperator = LeadingOperator(value);
      string trailingOperator = TrailingOperator(value);

      value = string.IsNullOrEmpty(leadingOperator) ? value : value.Remove(0, 1);
      value = string.IsNullOrEmpty(trailingOperator) ? value : value.Substring(0, (value.Length - 1));

      if (trailingOperator == "%")
      {
        leadingOperator = " LIKE ";
      }
      else if (string.IsNullOrEmpty(leadingOperator))
      {
        leadingOperator = "=";
      }

      string clause = "[" + propName + "] " + leadingOperator + " '" + value + "' " + trailingOperator;

      clause = clause.Replace("% '", "'%");
      clause = clause.Replace("' %", "%'");

      return clause;
    }

    public static string BuildQuery(JObject record, List<Column> columns)
    {
      Dictionary<string, string> colNameToPsedonym = columns.ToDictionary(x => x.ColumnName, x => x.Pseudonym);

      string query = "select top 100";

      List<JProperty> properties = record.Properties().ToList();

      int count = 0;
      for (int i = 0; i < properties.Count; i++)
      {
        string propName = properties[i].Name;

        if (propName == Configurations.ProtectedPropertyTableName)
        {
          continue;
        }

        propName = propName.ToLower();

        query += count == 0 ? " [" + colNameToPsedonym[propName] + "]" : ", [" + colNameToPsedonym[propName] + "]";
        count += 1;
      }

      query += " from [" + (string)record.SelectToken(Configurations.ProtectedPropertyTableName) + "] ";

      for (int i = 0; i < properties.Count; i++)
      {
        object val = (string)record.SelectToken(properties[i].Name);
        if (val == null || string.IsNullOrEmpty(val.ToString()) || string.IsNullOrWhiteSpace(val.ToString()))
        {
          continue;
        }

        string propName = properties[i].Name;

        if (propName == Configurations.ProtectedPropertyTableName)
        {
          continue;
        }

        propName = propName.ToLower();

        string clause = QueryHelpers.GetClause(colNameToPsedonym[propName], (string)val);

        query += query.Contains("where") ? " AND " + clause : "where " + clause;
      }

      return query;
    }

  }
}
