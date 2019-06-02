using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
  public class InsertObject
  {
    public string InsertStatements { get; set; }
    public Dictionary<string, string> ParameterMap { get; set; }
    public int RowCount { get; set; }
  }
}
