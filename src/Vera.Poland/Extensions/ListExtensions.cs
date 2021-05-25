using System.Collections.Generic;

namespace Vera.Poland.Extensions
{
  public static class ListExtensions
  {
    public static void Add<T>(this List<T> list, params T[] items)
    {
      foreach(var item in items)
        list.Add(item);
    }
  }
}
