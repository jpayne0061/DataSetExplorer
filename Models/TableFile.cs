using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
  public class TableFile
  {
    public string FileName { get; set; }
    public List<Column> Columns { get; set; }
    public string Description { get; set; }
    public string TableName { get; set; }

    public string GetTableName()
    {
      Regex rgx = new Regex("[^a-zA-Z]");

      string str = rgx.Replace(FileName.Split('.')[0], "");
      return str;
    }

    public void SetColumns(string fullPath)
    {
      string firstLine = File.ReadLines(fullPath).First();

      string[] columns = firstLine.Split(',');

      Columns = new List<Column>();

      foreach (var c in columns)
      {
        Columns.Add(new Column {
          ColumnName = c
        });
      }
    }

    public Dictionary<string, int> CreateColNameToIndexMap(string fullPath)
    {
      Dictionary<string, int> map = new Dictionary<string, int>();

      string firstLine = File.ReadLines(fullPath).First();

      string[] columns = firstLine.Split(',');

      for (int i = 0; i < columns.Length; i++)
      {
        map[columns[i]] = i;
      }

      return map;
    }

    public Dictionary<string, Column> CreateColNameToColumnMap()
    {
      Dictionary<string, Column> map = new Dictionary<string, Column>();

      foreach(var column in Columns)
      {
        map[column.ColumnName] = column;
      }

      return map;
    }

    public string GetInsertStatement(string fullPath)
    {
      string allInserts = "";

      Dictionary<string, int> map = CreateColNameToIndexMap(fullPath);

      Dictionary<string, Column> colMap = CreateColNameToColumnMap();

      using (StreamReader sr = new StreamReader(fullPath))
      using (var csv = new CsvReader(sr))
      {
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
          string insertStatement = "INSERT INTO " + TableName +
                      " ( " + String.Join(",", Columns.Select(x => x.ColumnName)) + ") VALUES (";

          for (var i = 0; i < Columns.Count; i++)
          {
            insertStatement += "'" + csv.GetField(Columns[i].ColumnName) + "'";
            if (i != Columns.Count - 1)
            {
              insertStatement += ",";
            }

          }
          insertStatement += @")

";

          allInserts += insertStatement;

        }
      }

      return allInserts;
    }

    public string CreateTableStatement(string fullPath, TableFile tableFile)
    {
      string createStatement = @"Create Table @TableName(";

      for (int i = 0; i < tableFile.Columns.Count; i++)
      {
        if (i == tableFile.Columns.Count - 1)
        {
          createStatement += tableFile.Columns[i].ColumnName + " " + tableFile.Columns[i].Type;
        }
        else
        {
          createStatement += tableFile.Columns[i].ColumnName + " " + tableFile.Columns[i].Type + ", ";
        }

        
      }

      createStatement.TrimEnd(' ');
      createStatement.TrimEnd(',');

      TableName = tableFile.GetTableName();

      createStatement = createStatement.Replace("@TableName", TableName);

      createStatement += ")";

      return createStatement;
    }

  }
}
