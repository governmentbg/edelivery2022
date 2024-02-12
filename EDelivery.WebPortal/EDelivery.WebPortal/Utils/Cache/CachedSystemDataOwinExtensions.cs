using System;
using System.Runtime.Caching;
using System.Web;

using ED.DomainServices.Profiles;

using Microsoft.Owin;

namespace EDelivery.WebPortal.Utils
{
    public static class CachedSystemDataOwinExtensions
    {
        public const string ProfileStatisticsKey = "ProfileStatistics";

        public static CachedProfileStatisticsData GetCachedProfileStatistics(this HttpContextBase httpContext)
        {
            return httpContext.GetOwinContext().GetCachedProfileStatistics();
        }

        public static CachedProfileStatisticsData GetCachedProfileStatistics(this IOwinContext owinContext)
        {
            return MemoryCacheExtensions.HomePageStatistics.AddOrGetExisting(
                ProfileStatisticsKey,
                () =>
                {
                    Profile.ProfileClient profileClient =
                        Grpc.GrpcClientFactory.CreateProfileClient();

                    GetStatisticsResponse r =
                        profileClient.GetStatistics(new Google.Protobuf.WellKnownTypes.Empty());

                    return new CachedProfileStatisticsData
                    {
                        PublicAdministrationsCount = r.PublicAdministrationsCount,
                        SocialOrganizationsCount = r.SocialOrganizationsCount,
                        LegalEntitiesCount = r.LegalEntitiesCount
                    };
                },
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(1)
                });
        }

        public static void ClearCachedProfileStatisticsData(this HttpContextBase httpContext)
        {
            httpContext.GetOwinContext().ClearCachedUserData();
        }

        public static void ClearCachedProfileStatisticsData(this IOwinContext owinContext)
        {
            MemoryCacheExtensions.HomePageStatistics.Remove(ProfileStatisticsKey);
        }
    }

    public class CachedProfileStatisticsData
    {
        public int PublicAdministrationsCount { get; set; }

        public int SocialOrganizationsCount { get; set; }

        public int LegalEntitiesCount { get; set; }
    }
}
