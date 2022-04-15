using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ED.Blobs
{
    public static class AuthorizationHelper
    {
        public static RequirementContext GetRequirementContext(object? resource)
        {
            if (resource is RequirementContext resourceReqCxt)
            {
                return resourceReqCxt;
            }

            if (resource is HttpContext httpContext)
            {
                if (httpContext.Items.TryGetValue(
                    nameof(RequirementContext),
                    out object? reqCxt))
                {
                    return (RequirementContext)(reqCxt ?? throw new Exception($"{nameof(reqCxt)} should not be null."));
                }
            }

            throw new Exception("Unsupported resource");
        }

        public static string? GetFromRouteOrQuery(this HttpContext httpContext, string key)
        {
            return httpContext.GetRouteData().Values[key] as string ??
                httpContext.Request.Query[key].FirstOrDefault();
        }
    }
}
