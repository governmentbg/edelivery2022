using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ED.EsbApi;

public static class HttpContextAuthExtensions
{
    public static string? GetFromRouteOrQuery(this HttpContext httpContext, string key)
    {
        return httpContext.GetRouteData().Values[key] as string ??
            httpContext.Request.Query[key].FirstOrDefault();
    }
}
