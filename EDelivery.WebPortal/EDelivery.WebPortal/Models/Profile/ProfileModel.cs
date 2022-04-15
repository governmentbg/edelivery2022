using EDelivery.WebPortal.Utils.Cache;

namespace EDelivery.WebPortal.Models
{
    public class ProfileModel
    {
        public ProfileModel(CachedLoginProfile loginProfile)
        {
            this.ProfileId = loginProfile.ProfileId;
            this.ProfileName = loginProfile.ProfileName;
            this.TargetGroupId = loginProfile.TargetGroupId;
            this.IsReadOnly = loginProfile.IsReadOnly;
            this.EnableMessagesWithCode = loginProfile.EnableMessagesWithCode;
            this.HasSEOS = loginProfile.HasSEOS;
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public int TargetGroupId { get; set; }

        public bool IsReadOnly { get; set; }

        public bool EnableMessagesWithCode { get; set; }

        public bool? HasSEOS { get; set; }
    }
}