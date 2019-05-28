using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SalaryExplorer.ExtensionMethods;

namespace SalaryExplorer.Models
{
  public class ColumnBase
  {
    public void PropsToLower()
    {
      PropertyInfo[] properties = GetType().GetProperties();

      for (int i = 0; i < properties.Length; i++)
      {
        string propName = properties[i].Name;
        string value = GetType().GetProperty(propName).GetValue(this).ToString();
        properties[i].SetValue(this, value.FirstCharToLower());
      }

    }

  }
}
