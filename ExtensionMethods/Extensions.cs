using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalaryExplorer.ExtensionMethods
{
  public static class Extensions
  {
    public static string LessThanOperator(this object obj)
    {
      return "";
    }

    //inspired by:
    //https://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-with-maximum-performance
    public static string FirstCharToLower(this string input)
    {
      switch (input)
      {
        case null: throw new ArgumentNullException(nameof(input));
        case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
        default: return input.First().ToString().ToLower() + input.Substring(1);
      }
    }
  }
}
