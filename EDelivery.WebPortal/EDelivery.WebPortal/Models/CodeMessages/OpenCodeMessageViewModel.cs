using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class OpenCodeMessageViewModel
    {
        [RequiredRes]
        [RegularExpression(@"\s*\w{8}[\-\s]*\w{4}[\-\s]*\w{4}[\-\s]*\w{4}[\-\s]*\w{12}[\s]*", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorInvalidAccessCode")]
        public string AccessCode { get; set; }
    }
}