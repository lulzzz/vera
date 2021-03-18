using System.Collections.Generic;
using System.Diagnostics;

namespace Vera.Norway.Extensions
{
    public static class CollectionExtensions
    {
        [DebuggerStepThrough]
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }
    }
}
