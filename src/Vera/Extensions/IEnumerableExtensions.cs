using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vera.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> list, Func<T, TKey> lookup)
        {
            return list.Distinct(new DistinctByEqualityComparer<T, TKey>(lookup));
        }

        private class DistinctByEqualityComparer<T, TKey> : IEqualityComparer<T>
        {
            private readonly Func<T, TKey> _lookup;

            public DistinctByEqualityComparer(Func<T, TKey> lookup) => _lookup = lookup;

            public bool Equals(T x, T y) => _lookup(x).Equals(_lookup(y));
            public int GetHashCode(T obj) => _lookup(obj).GetHashCode();
        }
    }
}
