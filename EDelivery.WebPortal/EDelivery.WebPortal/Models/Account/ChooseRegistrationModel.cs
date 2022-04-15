using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Enums;

namespace EDelivery.WebPortal.Models
{
    public class ChooseRegistrationModel
    {
        [Required(ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorRequiredChooseRegistration")]
        public eRegistrationType? RegistrationType { get; set; }

        public TargetGroupId TargetGroupId { get; set; }
    }
}