using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;
using Vera.Azure.Stores;

namespace Vera.Azure.Extensions
{
    public static class FeedIteratorExtensions
    {
        public static async Task<T> FirstOrDefault<T>(this IQueryable<Document<T>> queryable)
            where T: class
            => await queryable
                .Take(1)
                .FirstOrDefault<T, Document<T>>();

        public static async Task<T> FirstOrDefault<T>(this IQueryable<TypedDocument<T>> queryable)
            where T : class
            => await queryable
                .Take(1)
                .FirstOrDefault<T, TypedDocument<T>>();

        public static async Task<ICollection<T>> ToListAsync<T>(this IQueryable<Document<T>> queryable)
            where T : class
            => await queryable.ToListAsync<T, Document<T>>();

        public static async Task<ICollection<T>> ToListAsync<T>(this IQueryable<TypedDocument<T>> queryable)
            where T : class
            => await queryable.ToListAsync<T, TypedDocument<T>>();

        private static async Task<T?> FirstOrDefault<T, TK>(this IQueryable<TK> queryable)
            where T : class
            where TK : IDocument<T>
        {
            using var iterator = queryable.ToFeedIterator();
            var response = await iterator.ReadNextAsync();
            return response.Select(x => x.Value).FirstOrDefault();
        }

        private static async Task<ICollection<T>> ToListAsync<T, TK>(this IQueryable<TK> queryable)
            where T : class
        where TK : IDocument<T>
        {
            using var iterator = queryable.ToFeedIterator();
            var all = new List<T>();

            while (iterator.HasMoreResults)
            {
                var pagedResults = await iterator.ReadNextAsync();
                all.AddRange(pagedResults.Select(item => item.Value));
            }

            return all;
        }
    }
}
