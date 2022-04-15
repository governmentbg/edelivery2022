using System;
using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;
using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class CommonDataModel
    {
        public CommonDataModel()
        {
        }

        public CommonDataModel(
            int profileId,
            int targetGroupId,
            string email,
            string phone,
            string residence,
            bool syncNotificationSettings)
        {
            this.ProfileId = profileId;
            this.TargetGroupId = targetGroupId;
            this.Email = email;
            this.Phone = phone;
            this.Residence = residence;
            this.SyncNotificationSettings = syncNotificationSettings;
        }

        public int ProfileId { get; set; }

        public int TargetGroupId { get; set; }

        [RequiredRes]
        public string Residence { get; set; }

        [RequiredRes]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Email { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.PhoneRegex, ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidPhone", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Phone { get; set; }

        public bool SyncNotificationSettings { get; set; }

        public bool IsValidMobilePhone =>
            Regex.IsMatch(this.Phone, SystemConstants.ValidMobilePhone);
    }
}
