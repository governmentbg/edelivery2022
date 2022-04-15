using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ED.Blobs
{
    public static class HttpContextAuthExtensions
    {
        public static int? GetLoginId(this HttpContext httpContext)
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity!;

            if (!identity.IsAuthenticated)
            {
                return null;
            }

            string userIdString = 
                identity.FindFirst(ClaimTypes.NameIdentifier)
                ?.Value
                ?? throw new Exception("Missing userId claim.");

            if (!int.TryParse(userIdString, out var userId))
            {
                throw new Exception("The userId claim could not be parsed as a string.");
            }

            return userId;
        }
    }
}
