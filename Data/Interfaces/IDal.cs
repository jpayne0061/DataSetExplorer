using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.Data.Interfaces
{
  public interface IDal<T>
  {
    List<T> GetData(Dictionary<string, object> parameters, string proc, string connStr);
  }
}
