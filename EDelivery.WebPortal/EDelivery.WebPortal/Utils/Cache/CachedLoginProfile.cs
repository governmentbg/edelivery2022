using System;

namespace EDelivery.WebPortal.Utils.Cache
{
    public class CachedLoginProfile
    {
        public CachedLoginProfile()
        {
        }

        public CachedLoginProfile(
            ED.DomainServices.Profiles.GetLoginProfilesResponse.Types.LoginProfile loginProfile)
        {
            this.ProfileId = loginProfile.ProfileId;
            this.ProfileGuid = Guid.Parse(loginProfile.ProfileGuid);
            this.ProfileName = loginProfile.ProfileName;
            this.Email = loginProfile.Email;
            this.Phone = loginProfile.Phone;
            this.Identifier = loginProfile.Identifier;
            this.EnableMessagesWithCode = loginProfile.EnableMessagesWithCode ?? false;
            this.TargetGroupId = loginProfile.TargetGroupId;
            this.IsDefault = loginProfile.IsDefault;
            this.IsReadOnly = loginProfile.IsReadOnly;
            this.DateAccessGranted = loginProfile.DateAccessGranted.ToLocalDateTime();
            this.IsPassive = false;

            if (this.TargetGroupId != (int)Enums.TargetGroupId.PublicAdministration)
            {
                this.HasSEOS = false;
            }
        }

        public int ProfileId { get; set; }

        public Guid ProfileGuid { get; set; }

        public string ProfileName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Identifier { get; set; }

        public bool EnableMessagesWithCode { get; set; }

        public int TargetGroupId { get; set; }

        public bool IsDefault { get; set; }

        public bool IsReadOnly { get; set; }

        public DateTime DateAccessGranted { get; set; }

        public bool? HasSEOS { get; set; }

        public bool IsPassive { get; set; }

        public static CachedLoginProfile GetFakeDefaultProfile(string name)
        {
            CachedLoginProfile profile = new CachedLoginProfile()
            {
                ProfileId = 0,
                ProfileGuid = Guid.Empty,
                ProfileName = name,
                Email = string.Empty,
                Phone = string.Empty,
                Identifier = Guid.Empty.ToString(),
                EnableMessagesWithCode = false,
                TargetGroupId = (int)Enums.TargetGroupId.Individual,
                IsDefault = true,
                IsReadOnly = false,
                DateAccessGranted = DateTime.MinValue,
                IsPassive = true
            };

            return profile;
        }
    }
}