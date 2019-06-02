using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
  public class Column : ColumnBase
  {
    public string ColumnName { get; set; }
    public string DisplayName { get; set; }
    public string Visible { get; set; }
    public string TableName { get; set; }
    public string Type { get; set; }
    public string Pseudonym { get; set; }
    public string DataSetTitle { get; set; }
  }
}
