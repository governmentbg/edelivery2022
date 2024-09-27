using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class RegisterLegalEntityViewModel
    {
        public RegisterLegalEntityViewModel()
        {
            this.EmailNotifications = true;
            this.PhoneNotifications = false;
        }

        [RequiredRes]
        public int? FileId { get; set; }

        public string FileName { get; set; }

        [RequiredRes]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string EmailAddress { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.PhoneRegex, ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidPhone", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string PhoneNumber { get; set; }

        public bool EmailNotifications { get; set; }

        public bool PhoneNotifications { get; set; }

        [RequireTrue(ErrorMessageResourceName = "ErrorAcceptLicenceAgreement", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public bool LicenceAgreement { get; set; }
    }
}
