using System.Collections.Generic;
using System.Linq;

using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Utils.Cache;

namespace EDelivery.WebPortal.Utils
{
    public class CachedUserData
    {
        public int LoginId { get; set; }

        public List<CachedLoginProfile> Profiles { get; set; }

        public string DefaultProfileName { get; }

        public int ActiveProfileId { get; set; }

        public BreadCrumb BreadCrumb { get; set; }

        public CachedLoginProfile ActiveProfile
        {
            get
            {
                CachedLoginProfile loginProfile =
                    Profiles.Find(x => x.ProfileId == ActiveProfileId);

                return loginProfile;
            }
        }

        public CachedUserData(
            int loginId,
            string defaultProfileName,
            List<CachedLoginProfile> profiles)
        {
            if (!profiles.Any(e => e.IsDefault))
            {
                CachedLoginProfile fakeDefaultProfile =
                    CachedLoginProfile.GetFakeDefaultProfile(defaultProfileName);

                profiles.Add(fakeDefaultProfile);
            }

            CachedLoginProfile defaultProfile = profiles
               .Single(p => p.IsDefault);

            this.LoginId = loginId;
            this.Profiles = profiles;
            this.DefaultProfileName = defaultProfileName;
            this.ActiveProfileId = defaultProfile.ProfileId;

            this.BreadCrumb = new BreadCrumb(
                defaultProfile.ProfileName,
                Utils.GetActionUrl("Index", "Profile"),
                EDeliveryResources.Common.BreadCrumbHomePage);
        }
    }
}