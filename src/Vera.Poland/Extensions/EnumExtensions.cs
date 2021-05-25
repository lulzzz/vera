using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Vera.Poland.Extensions
{
  public static class EnumExtensions
  {
    public static string ToStringValue<TSource, TEnum>(this TSource source, Expression<Func<TSource, TEnum>> selector)
      where TEnum : struct
    {
      var valueToCheck = selector.Compile()(source);

      var integerValue = Convert.ToInt32(valueToCheck);

      var stringValue = integerValue.ToString(CultureInfo.InvariantCulture);

      return stringValue;
    }
  }
}