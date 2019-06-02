using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
  public class Query
  {
    public string Body { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
  }
}
