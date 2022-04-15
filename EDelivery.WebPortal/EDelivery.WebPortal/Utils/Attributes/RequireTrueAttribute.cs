using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequireTrueAttribute : ValidationAttribute, IClientValidatable
    {
        public RequireTrueAttribute()
        { }

        public RequireTrueAttribute(string errorMessage) : base(errorMessage)
        { }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is Boolean && (bool)value == true) return ValidationResult.Success;
            return new ValidationResult(this.ErrorMessageString);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule compareRule = new ModelClientValidationRule();
            compareRule.ErrorMessage = this.ErrorMessageString;
            compareRule.ValidationType = "requiretrue";

            compareRule.ValidationParameters.Add("propertyname", metadata.PropertyName);
            yield return compareRule;
        }
    }
}