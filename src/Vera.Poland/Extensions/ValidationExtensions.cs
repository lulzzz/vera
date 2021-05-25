using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vera.Poland.Extensions
{
  public static class ValidationExtensions
  {
    public static void GuardNotNull<TSource, TEnum>(this TSource source, Expression<Func<TSource, TEnum>> selector)
      where TEnum : struct
    {
      var enumType = typeof(TEnum);
      CheckEnumProperFormat(enumType);

      var propertyName = (selector.Body as MemberExpression ?? ((UnaryExpression)selector.Body).Operand as MemberExpression)?.Member.Name;
      var valueToCheck = selector.Compile()(source);
      var propertyIsInitialized = !EqualityComparer<TEnum>.Default.Equals(valueToCheck, default);

      if (!propertyIsInitialized)
      {
        throw new ArgumentNullException(propertyName);
      }
    }

    /// <summary>
    /// Check that we have an enum starting with a value None = 0
    ///
    /// This is needed because we need to check if the enum is initialized i.e. doesn't have default value 0
    /// </summary>
    private static void CheckEnumProperFormat(Type enumType)
    {
      var names = Enum.GetNames(enumType).ToList();
      var values = Enum.GetValues(enumType).Cast<int>().ToList();

      var hasProperForm = false;

      for (var i = 0; i < names.Count; i++)
      {
        if (names[i].ToLowerInvariant() == "none" && values[i] == 0)
        {
          hasProperForm = true;
          break;
        }
      }

      if (!hasProperForm)
      {
        throw new InvalidOperationException("Enum needs to have the first entry: None = 0");
      }
    }
  }
}
