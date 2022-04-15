using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class RequiredResAttribute : RequiredAttribute, IClientValidatable
    {
        public RequiredResAttribute()
            : base()
        {
            this.ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages);
            this.ErrorMessageResourceName = "ErrorRequired";
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ModelMetadata metadata,
            ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessageString,
                ValidationType = "required"
            };
        }
    }
}
