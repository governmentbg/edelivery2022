using System;
using System.Collections.Generic;
using System.Linq;

namespace ED.Domain
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WithOffsetAndLimit<T>(
            this IEnumerable<T> query,
            int? offset,
            int? limit)
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

        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sequences)
        {
            return sequences.SelectMany(x => x);
        }

        public static IEnumerable<IEnumerable<T>> GroupAdjacentBy<T>(
            this IEnumerable<T> source,
            Func<T, T, bool> predicate)
        {
            using var e = source.GetEnumerator();
            if (e.MoveNext())
            {
                var list = new List<T> { e.Current };
                var pred = e.Current;
                while (e.MoveNext())
                {
                    if (predicate(pred, e.Current))
                    {
                        list.Add(e.Current);
                    }
                    else
                    {
                        yield return list;
                        list = new List<T> { e.Current };
                    }
                    pred = e.Current;
                }
                yield return list;
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
