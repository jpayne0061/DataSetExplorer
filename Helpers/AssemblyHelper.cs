using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace SalaryExplorer.Helpers
{
  public static class AssemblyHelper
  {
    public static object ExecFromAssembly(string dllPath, string fullyQualifiedPath, string methodName)
    {
      Assembly asm =
  AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
      // Invoke the RoslynCore.Helper.CalculateCircleArea method passing an argument
      double radius = 10;
      object result =
        asm.GetType(fullyQualifiedPath).GetMethod(methodName).
        Invoke(null, new object[] { radius });

      return result;
    }
  }
}
