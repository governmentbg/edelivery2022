using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class NewCodeMessageViewModel
    {
        [RequiredRes]
        [PersonalIdentifierValidation(ErrorMessageResourceName = "ErrorMessageInvalidEGNorLNCh", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Identifier { get; set; }

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
        public string Email { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.PhoneRegex, ErrorMessage = null, ErrorMessageResourceName = "ErrorInvalidPhone", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Phone { get; set; }

        [RequiredRes]
        [StringLength(255, ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorMessageFieldLength")]
        public string Subject { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public string TemplateValuesAsJson { get; set; }

        public string TemplateErrorsAsJson { get; set; }

        public int CurrentProfileId { get; set; }
    }
}
