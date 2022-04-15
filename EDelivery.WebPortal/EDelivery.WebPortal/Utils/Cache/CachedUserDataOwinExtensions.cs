using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Principal;
using System.Web;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.SeosService;
using EDelivery.WebPortal.Utils.Cache;

using Microsoft.AspNet.Identity;
using Microsoft.Owin;

namespace EDelivery.WebPortal.Utils
{
    public static class CachedUserDataOwinExtensions
    {
        public static CachedUserData GetCachedUserData(this HttpContextBase httpContext)
        {
            return httpContext.GetOwinContext().GetCachedUserData();
        }

        public static CachedUserData GetCachedUserData(this IOwinContext owinContext)
        {
            IPrincipal user = owinContext.Request.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return MemoryCacheExtensions.UserDataCache.AddOrGetExisting(
                    user.Identity.Name,
                    () => InitUserData(
                        user.Identity.GetUserId<int>(),
                        user.Identity.Name),
                    new CacheItemPolicy
                    {
                        SlidingExpiration = AuthConfig.AuthenticationCookieExpiration
                    });
            }
            else
            {
                return null;
            }
        }

        public static void ClearCachedUserData(this HttpContextBase httpContext)
        {
            httpContext.GetOwinContext().ClearCachedUserData();
        }

        public static void ClearCachedUserData(this IOwinContext owinContext)
        {
            IPrincipal user = owinContext.Request.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                MemoryCacheExtensions.UserDataCache.Remove(user.Identity.Name);
            }
        }

        public static Dictionary<string, bool> GetSeosAvailability(
            string[] identifiers)
        {
            try
            {
                using (SEOSPostServiceClient seosClient = new SEOSPostServiceClient())
                {
                    return seosClient.HasSeos(identifiers);
                }
            }
            catch
            {
                return null;
            }
        }

        private static CachedUserData InitUserData(
            int userId,
            string userName)
        {
            ED.DomainServices.Profiles.Profile.ProfileClient profileClient =
                Grpc.GrpcClientFactory.CreateProfileClient();

            ED.DomainServices.Profiles.GetLoginProfilesResponse response =
                profileClient.GetLoginProfiles(
                    new ED.DomainServices.Profiles.GetLoginProfilesRequest
                    {
                        LoginId = userId
                    });

            string defaultProfileName = response
                .LoginProfiles
                .SingleOrDefault(x => x.IsDefault)?
                .ProfileName
                    ?? userName;

            List<CachedLoginProfile> cachedLoginProfiles = response
                .LoginProfiles
                .Select(p => new CachedLoginProfile(p))
                .ToList();

            Dictionary<string, bool> seosAvailability = GetSeosAvailability(
                cachedLoginProfiles.Select(e => e.Identifier).ToArray());

            if (seosAvailability != null)
            {
                foreach (CachedLoginProfile profile in cachedLoginProfiles.Where(e => e.TargetGroupId == (int)TargetGroupId.PublicAdministration))
                {
                    profile.HasSEOS =
                        seosAvailability.ContainsKey(profile.Identifier)
                        && seosAvailability[profile.Identifier];
                }
            }

            return new CachedUserData(userId, defaultProfileName, cachedLoginProfiles);
        }
    }
}
