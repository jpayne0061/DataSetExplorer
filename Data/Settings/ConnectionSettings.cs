using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Data.Settings
{
  public static class ConnectionSettings
  {
    public static readonly string SalaryConnString =
      @"Data Source=YourServer;Initial Catalog=YourDatabaseName;
      Integrated Security=False; User Id=dataUser; Password=password;";

  }
}
