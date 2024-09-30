using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class LoginProfileSettingsViewModel
    {
        public LoginProfileSettingsViewModel()
        {
        }

        public LoginProfileSettingsViewModel(
            int profileId,
            int targetGroupId,
            ED.DomainServices.Profiles.GetSettingsResponse settings)
        {
            this.ProfileId = profileId;
            this.TargetGroupId = targetGroupId;
            this.IsEmailNotificationEnabled = settings.IsEmailNotificationEnabled;
            this.IsEmailNotificationOnDeliveryEnabled = settings.IsEmailNotificationOnDeliveryEnabled;
            this.IsPhoneNotificationEnabled = settings.IsPhoneNotificationEnabled;
            this.IsPhoneNotificationOnDeliveryEnabled = settings.IsPhoneNotificationOnDeliveryEnabled;
            this.Email = settings.Email;
            this.Phone = settings.Phone;
        }

        public int ProfileId { get; set; }

        public int TargetGroupId { get; set; }

        public bool IsEmailNotificationEnabled { get; set; }

        public bool IsEmailNotificationOnDeliveryEnabled { get; set; }

        public bool IsPhoneNotificationEnabled { get; set; }

        public bool IsPhoneNotificationOnDeliveryEnabled { get; set; }

        [RequiredRes]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Email { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.PhoneRegex, ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidPhone", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Phone { get; set; }

        public bool IsValidMobilePhone =>
            Regex.IsMatch(this.Phone, SystemConstants.ValidMobilePhone);
    }
}
