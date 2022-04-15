using EDelivery.WebPortal.Utils;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Authorization
{
    public static class AuthorizationHelper
    {
        private static string GetAuthorizationKey(object resource, string requirement)
        {
            if (resource is AuthorizationContext)
            {
                var httpContext = ((AuthorizationContext)resource).HttpContext;

                string identityCookie =
                    httpContext.Request.Cookies[AuthConfig.EDeliveryIdentityCookieName]?.Value;

                if (!string.IsNullOrEmpty(identityCookie))
                {
                    string rqCtxString = httpContext.GetOwinContext().Get<RequirementContext>().ToString();
                    string key = requirement + "/" + rqCtxString + "/" + identityCookie;
                    return key;
                }
            }

            // if we cant get the Identity cookie the call is not cacheable
            return null;
        }

        public static bool AddOrGetCachedRequirementSucceeds<TRequirement>(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Func<AuthorizationHandlerContext, TRequirement, bool> handleRequirement)
        {
            string key = GetAuthorizationKey(context.Resource, typeof(TRequirement).Name);

            if (string.IsNullOrEmpty(key))
            {
                // no caching
                return handleRequirement(context, requirement);
            }

            return MemoryCacheExtensions.AuthorizationCache.AddOrGetExisting(
                key,
                () => handleRequirement(context, requirement),
                new CacheItemPolicy
                {
                    // the cookie string is part of the key and will be valid for
                    // atmost a TimeSpan of AuthenticationCookieExpiration
                    AbsoluteExpiration = DateTimeOffset.Now.Add(AuthConfig.AuthenticationCookieExpiration)
                });
        }

        public static RequirementContext GetRequirementContext(object resource)
        {
            if (resource is RequirementContext)
            {
                return (RequirementContext)resource;
            }

            if (resource is AuthorizationContext)
            {
                IOwinContext owinContext = ((AuthorizationContext)resource).HttpContext.GetOwinContext();
                return owinContext.Get<RequirementContext>();
            }

            throw new Exception("Unsupported resource");
        }

        public static int GetInt32RouteOrQueryParam(this AuthorizationContext authorizationContext, string key)
        {
            var request = authorizationContext.HttpContext.Request;
            var param =
                request.RequestContext.RouteData.Values[key]
                ?? request.QueryString[key];
            return Convert.ToInt32(param);
        }

        public static int GetInt32FormValue(this AuthorizationContext authorizationContext, string key)
        {
            var formValue = authorizationContext.HttpContext.Request.Form[key];
            return Convert.ToInt32(formValue);
        }

        public static async Task<bool> CanAdministerProfileAsync(this HttpContextBase httpContext)
        {
            IOwinContext owinContext = httpContext.GetOwinContext();
            CachedUserData cachedUserData = owinContext.GetCachedUserData();
            RequirementContext requirementContext = new RequirementContext(
                new Dictionary<string, object>
                {
                    { RequirementContextItems.LoginId, cachedUserData.LoginId },
                    { RequirementContextItems.ProfileId, cachedUserData.ActiveProfileId },
                });

            return await owinContext.GetAuthorizationService()
                .AuthorizeAsync(
                    (ClaimsPrincipal)httpContext.User,
                    requirementContext,
                    Policies.AdministerProfile);
        }
    }
}
