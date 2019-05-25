using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Models
{
public class RecordInput
{
  public string SalaryDataID { get; set; }
  public string CalendarYear { get; set; }
  public string EmployeeName { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string Department { get; set; }
  public string JobTitle { get; set; }
  public string AnnualRate { get; set; }
  public string RegularRate { get; set; }
  public string OvertimeRate { get; set; }
  public string IncentiveAllowance { get; set; }
  public string Other { get; set; }
  public string YearToDate { get; set; }
}
}
