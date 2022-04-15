using System.Collections;
using System.Collections.Generic;
using Mapster;

namespace ED.EsbApi;

public static class MapsterExtensions
{
    public static IEnumerable<TDestination> ProjectToType<TDestination>(
            this IEnumerable source)
    {
        foreach (var item in source)
        {
            yield return item.Adapt<TDestination>();
        }
    }
}
