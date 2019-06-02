using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using SalaryExplorer.Data.Get;
using SalaryExplorer.Data.Settings;
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
            }
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

      return insertObject;
    }

    public async void InsertColumns(int insertedId, Dal dal)
    {
      for (int i = 0; i < Columns.Count; i++)
      {
        string pseudonym = Guid.NewGuid().ToString();
        Columns[i].Pseudonym = pseudonym;
        string insertColumnprocName = "InsertTableColumn";
        Dictionary<string, string> columnParam = new Dictionary<string, string>();
        columnParam["@ColumnName"] = Columns[i].ColumnName;
        columnParam["@Visible"] = Columns[i].Visible == null ? "0" : Columns[i].Visible;
        columnParam["@DisplayName"] = Columns[i].DisplayName == null ? Columns[i].ColumnName : Columns[i].DisplayName.Replace("\"", "");
        columnParam["@DataTableId"] = insertedId.ToString();
        columnParam["@Type"] = Columns[i].Type ?? "varchar(500)";
        columnParam["@Pseudonym"] = pseudonym;
        await dal.NonQuery(columnParam, insertColumnprocName, ConnectionSettings.InsertConnString);
      }
    }

    public async void CreateTableAndLoadData(string tableGuid, string pathToSave, string fullPath, Dal dal, IHubContext<SignalRHub> signalHubContext)
    {
      string createTableStatement = CreateTableStatement(fullPath, this, tableGuid);

      try
      {
        await dal.NonQueryByStatement(createTableStatement, ConnectionSettings.InsertConnString);
      }
      catch (Exception ex)
      {
        await signalHubContext.Clients.All.SendAsync("data update", "table creation failed: " + ex.Message);
        throw;
      }

      await signalHubContext.Clients.All.SendAsync("data update", "table created");

      SqlConnection conn = new SqlConnection(ConnectionSettings.InsertConnString);

      InsertObject insertObject;

      try
      {
        insertObject = GetInsertStatement(fullPath, tableGuid);
      }
      catch (Exception ex)
      {
        await signalHubContext.Clients.All.SendAsync("data update", "DML statements failed: " + ex.Message);
        throw;
      }

      await signalHubContext.Clients.All.SendAsync("rows found", insertObject.RowCount);

      string[] splitStatements = insertObject.InsertStatements.Split(';');

      InsertDataRows(splitStatements, dal, signalHubContext, insertObject);

      await signalHubContext.Clients.All.SendAsync("complete", insertObject.RowCount);
    }

    public async Task<int> InsertDataTable(Dal dal, string tableGuid)
    {
      Columns.ForEach(col =>
        col.ColumnName = col.ColumnName.Replace("\"", "").Replace("'", "").Replace("'", "").ToLower());

      string procName = "InsertDataTable";

      Dictionary<string, string> param = new Dictionary<string, string>();
      param["@TableName"] = GetTableName();
      param["@Description"] = Description == null ? "" : Description;
      param["@Guid"] = tableGuid;
      param["@DataSetTitle"] = DataSetTitle;
      int id = await dal.NonQuery(param, procName, ConnectionSettings.InsertConnString);
      return id;
    }

    public async void InsertDataRows(string[] splitStatements, Dal dal, IHubContext<SignalRHub> signalHubContext, InsertObject insertObject)
    {
      int maxRows = (int)((decimal)2000 / Columns.Count);

      int numStatements = splitStatements.Length - 1;
      int numToSkip = 0;

      int numToProcess = Math.Min(3000, maxRows);

      while (numStatements > 0)
      {
        string insert = string.Join("", splitStatements.Skip(numToSkip).Take(numToProcess));
        await dal.ExecuteInsertObject(this, insert, insertObject.ParameterMap, ConnectionSettings.InsertConnString, numToProcess, numToSkip, splitStatements.Length - 1);
        await signalHubContext.Clients.All.SendAsync("row update", numToSkip);
        numToSkip += maxRows;
        numStatements -= maxRows;
      }
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

    public async void RollBackInserts(int insertedId, Dal dal, string tableGuid)
    {
      string deleteColumns = "DELETE FROM TableColumns WHERE DataTableId = " + insertedId.ToString();
      string deleteDataTable = "DELETE FROM DataTable WHERE DataTableId = " + insertedId.ToString();
      string dropTable = "DROP TABLE IF EXISTS " + "[" + tableGuid + "]";

      await dal.NonQueryByStatement(dropTable, ConnectionSettings.SaConnString);

      await dal.NonQueryByStatement(deleteColumns, ConnectionSettings.SaConnString);
      await dal.NonQueryByStatement(deleteDataTable, ConnectionSettings.SaConnString);
    }
  }
}
