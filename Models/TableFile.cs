using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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
    public string DataSetTitle { get; set; }
    public string TableGuid { get; set; }

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

    public InsertObject GetInsertStatement(string fullPath, string tableGuid)
    {
      //foreach row...
      //try parse each data type, if fail, then NULL
      //if read fails, then skip row

      StringBuilder allInserts = new StringBuilder("");

      Dictionary<string, int> map = CreateColNameToIndexMap(fullPath);

      Dictionary<string, Column> colMap = CreateColNameToColumnMap();

      Dictionary<string, string> commandParameters = new Dictionary<string, string>();
      int count = 0;

      using (StreamReader sr = new StreamReader(fullPath))
      using (var csv = new CsvReader(sr))
      {
        csv.Configuration.BadDataFound = context =>
        {
            //skip
        };

        csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
        try
        {
          csv.Read();
          csv.ReadHeader();
          while (csv.Read())
          {
            string insertStatement = "INSERT INTO " + "[" + tableGuid + "]" +
                      " ( " + String.Join(",", Columns.Select(x => "[" + x.Pseudonym + "]")) + ") VALUES (" +

            String.Join(",", Columns.Select(x => "@" + count.ToString() + x.Pseudonym.Replace("-", ""))) + ");";

            for (var i = 0; i < Columns.Count; i++)
            {
              commandParameters["@" + count.ToString() + Columns[i].Pseudonym.Replace("-", "")] = csv.GetField(Columns[i].ColumnName);

              //insertStatement += "'" + csv.GetField(Columns[i].ColumnName) + "'";
              //if (i != Columns.Count - 1)
              //{
              //  insertStatement += ",";
              //}
            }
//            insertStatement += @")
//;" ;

            allInserts.Append(insertStatement);
            count += 1;
          }
        }
        catch (Exception ex)
        {
          //do something
        }
      }

      string allInsertStatements = allInserts.ToString();

      InsertObject insertObject = new InsertObject();
      insertObject.ParameterMap = commandParameters;
      insertObject.InsertStatements = allInsertStatements.TrimEnd(';');
      insertObject.RowCount = count;
      //SqlCommand dbCommand = new SqlCommand(allInsertStatements, connection);

      //foreach(KeyValuePair<string, string> kvp in commandParameters)
      //{
      //  dbCommand.Parameters.AddWithValue(kvp.Key, kvp.Value);
      //}

      return insertObject;
    }

    public string CreateTableStatement(string fullPath, TableFile tableFile, string tableGuid)
    {
      string createStatement = @"Create Table @TableName(";

      for (int i = 0; i < tableFile.Columns.Count; i++)
      {
        string type = "varchar(500)";

        switch (tableFile.Columns[i].Type)
        {
          case "1":
            type = "varchar(500)";
            break;
          case "2":
            type = "int";
            break;
          case "3":
            type = "decimal";
            break;
          case "4":
            type = "money";
            break;
          case "5":
            type = "datetime";
            break;
        }

        if (i == tableFile.Columns.Count - 1)
        {
          createStatement += "[" + tableFile.Columns[i].Pseudonym + "] " + type;
        }
        else
        {
          createStatement += "[" + tableFile.Columns[i].Pseudonym + "] " + type + ", ";
        }
      }

      createStatement.TrimEnd(' ');
      createStatement.TrimEnd(',');

      TableName = tableFile.GetTableName();

      createStatement = createStatement.Replace("@TableName", "[" + tableGuid + "]");

      createStatement += ")";

      return createStatement;
    }

  }
}
