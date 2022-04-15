using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class RequiredIfAttribute : ValidationAttribute, IClientValidatable
    {
        public string OtherPropertyName { get; set; }

        public object OtherPropertyValue { get; set; }

        public RequiredIfAttribute(string propertyName, object propertyValue)
        {
            this.OtherPropertyName = propertyName;
            this.OtherPropertyValue = propertyValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null) return ValidationResult.Success;
            var propVal = validationContext.ObjectType.GetProperty(OtherPropertyName).GetValue(validationContext.ObjectInstance);
            if (propVal.Equals(OtherPropertyValue))
            {
                return new ValidationResult(this.ErrorMessageString);
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule compareRule = new ModelClientValidationRule();
            compareRule.ErrorMessage = this.ErrorMessageString;
            compareRule.ValidationType = "requiredif";

            compareRule.ValidationParameters.Add("propertyname", metadata.PropertyName);
            compareRule.ValidationParameters.Add("comparename", OtherPropertyName);
            compareRule.ValidationParameters.Add("comparevalue", OtherPropertyValue);
            yield return compareRule;
        }
    }
}