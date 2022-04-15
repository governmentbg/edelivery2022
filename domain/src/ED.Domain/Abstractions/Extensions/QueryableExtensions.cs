using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WithOffsetAndLimit<T>(
            this IQueryable<T> query,
            int? offset, int? limit)
        {
            offset ??= 0;
            if (offset > 0)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query;
        }

        public static Task<IEnumerable<T>> UnionWithOffsetAndLimitAsync<T>(
            this IEnumerable<IQueryable<T>> queries,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            return UnionWithOffsetAndLimitAsync(
                queries,
                offset,
                limit,
                EqualityComparer<T>.Default,
                ct);
        }

        public static async Task<IEnumerable<T>> UnionWithOffsetAndLimitAsync<T>(
            this IEnumerable<IQueryable<T>> queries,
            int? offset,
            int? limit,
            IEqualityComparer<T> comparer,
            CancellationToken ct)
        {
            if (queries == null || !queries.Any())
            {
                throw new ArgumentException("Parameter should be non-null and non-empty", nameof(queries));
            }

            var next = queries.First();
            var rest = queries.Skip(1);

            if (!rest.Any())
            {
                // this is the final query, execute and return
                return await next.WithOffsetAndLimit(offset, limit).ToListAsync(ct);
            }

            offset ??= 0;
            if (offset > 0)
            {
                var count = next.Count();
                if (offset >= count)
                {
                    // the offset is beyond the current query item count,
                    // continue with the rest
                    return await rest.UnionWithOffsetAndLimitAsync(
                        offset - count,
                        limit,
                        ct);
                }
            }

            var items = await next.WithOffsetAndLimit(offset, limit).ToListAsync(ct);

            if (limit != null && items.Count == limit.Value)
            {
                // we've reached the limit, no more quiering needed
                return items;
            }

            var nextItems = await rest.UnionWithOffsetAndLimitAsync(
                0,
                limit == null ? null : limit - items.Count,
                ct);

            return items.Union(nextItems, comparer);
        }

        public static IQueryable<IdQO> MakeIdsQuery(
            this Microsoft.EntityFrameworkCore.DbContext dbContext,
            int[] ids)
        {
            return dbContext.Set<IdQO>()
                .FromSqlRaw(
                    "SELECT [Id] FROM OPENJSON(@idsJsonArray) WITH ([Id] INT '$')",
                    new SqlParameter("idsJsonArray", JsonSerializer.Serialize(ids)));
        }

        public static IQueryable<StringIdQO> MakeIdsQuery(
            this Microsoft.EntityFrameworkCore.DbContext dbContext,
            string[] ids)
        {
            return dbContext.Set<StringIdQO>()
                .FromSqlRaw(
                    "SELECT [Id] FROM OPENJSON(@idsJsonArray) WITH ([Id] NVARCHAR(MAX) '$')",
                    new SqlParameter("idsJsonArray", JsonSerializer.Serialize(ids)));
        }

        public static IQueryable<Id2QO> MakeIdsQuery(
            this Microsoft.EntityFrameworkCore.DbContext dbContext,
            (int id1, int id2)[] ids)
        {
            return dbContext.Set<Id2QO>()
                .FromSqlRaw(
                    "SELECT [Id1], [Id2] FROM OPENJSON(@idsJsonArray) WITH ([Id1] INT '$.Id1', [Id2] INT '$.Id2')",
                    new SqlParameter(
                        "idsJsonArray",
                        JsonSerializer.Serialize(
                            ids
                            .Select(t => new Id2QO() { Id1 = t.id1, Id2 = t.id2 })
                            .ToArray()
                        )
                    )
                );
        }

        public static IQueryable<Id4QO> MakeIdsQuery(
            this Microsoft.EntityFrameworkCore.DbContext dbContext,
            (int id1, int id2, int id3, int id4)[] ids)
        {
            return dbContext.Set<Id4QO>()
                .FromSqlRaw(
                    "SELECT [Id1], [Id2], [Id3], [Id4]" +
                    " FROM OPENJSON(@idsJsonArray) WITH ([Id1] INT '$.Id1', [Id2] INT '$.Id2', [Id3] INT '$.Id3', [Id4] INT '$.Id4')",
                    new SqlParameter(
                        "idsJsonArray",
                        JsonSerializer.Serialize(
                            ids
                            .Select(t =>
                                new Id4QO()
                                {
                                    Id1 = t.id1,
                                    Id2 = t.id2,
                                    Id3 = t.id3,
                                    Id4 = t.id4
                                })
                            .ToArray()
                        )
                    )
                );
        }

        public static async Task<TableResultVO<T>> ToTableResultAsync<T>(
            this IQueryable<T> query,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            T[] result = await query
                .WithOffsetAndLimit(offset, limit)
                .ToArrayAsync(ct);

            int length;
            if (limit.HasValue)
            {
                length = await query.CountAsync(ct);
            }
            else
            {
                length = result.Length + (offset ?? 0);
            }

            return new TableResultVO<T>(result, length);
        }

        public static async Task<TableResultVO<T>> ToTableResultAsync<TItem, T>(
            this IQueryable<TItem> query,
            Func<TItem, T> selector,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            TItem[] result = await query
                .WithOffsetAndLimit(offset, limit)
                .ToArrayAsync(ct);

            int length;
            if (limit.HasValue)
            {
                length = await query.CountAsync(ct);
            }
            else
            {
                length = result.Length + (offset ?? 0);
            }

            return new TableResultVO<T>(result.Select(selector).ToArray(), length);
        }
    }
}
