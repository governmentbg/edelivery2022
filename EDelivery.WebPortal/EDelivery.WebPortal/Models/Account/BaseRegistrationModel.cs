using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class BaseRegistrationModel
    {
        public BaseRegistrationModel()
        {
        }

        public bool LockNames { get; set; }

        [RequiredRes]
        [StringLength(50, ErrorMessageResourceName = "ErrorMessageFieldLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "ErrorMessageCyrilicRequired", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string FirstName { get; set; }

        [RequiredRes]
        [StringLength(50, ErrorMessageResourceName = "ErrorMessageFieldLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "ErrorMessageCyrilicRequired", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string MiddleName { get; set; }

        [RequiredRes]
        [StringLength(50, ErrorMessageResourceName = "ErrorMessageFieldLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "ErrorMessageCyrilicRequired", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string LastName { get; set; }

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

        [RequireTrue(ErrorMessageResourceName = "ErrorAcceptGDPRAgreement", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public bool GDPRAgreement { get; set; }

        [StringLength(50, ErrorMessageResourceName = "ErrorUserNameLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string UserName { get; set; }

        [RequiredRes]
        public string Address { get; set; }

        public bool CreateProfile { get; set; } = true;
    }
}
