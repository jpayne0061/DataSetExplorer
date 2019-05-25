using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
  public class Record
  {
    public int SalaryDataID { get; set; }
    public int CalendarYear { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public string JobTitle { get; set; }
    public decimal AnnualRate { get; set; }
    public decimal RegularRate { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal IncentiveAllowance { get; set; }
    public decimal Other { get; set; }
    public decimal YearToDate { get; set; }

  }
}
