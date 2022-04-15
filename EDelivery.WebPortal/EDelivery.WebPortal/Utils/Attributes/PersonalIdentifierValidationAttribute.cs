using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PersonalIdentifierValidationAttribute : ValidationAttribute, IClientValidatable
    {
        public PersonalIdentifierValidationAttribute()
        { }

        public PersonalIdentifierValidationAttribute(string errorMessage) : base(errorMessage)
        { }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string egnOrLnch = value as string;
            if (string.IsNullOrEmpty(egnOrLnch))
            {
                return ValidationResult.Success;
            }

            if (!TextHelper.IsEGN(egnOrLnch) && !TextHelper.IsLNCh(egnOrLnch))
            {
                return new ValidationResult(this.ErrorMessageString);
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule compareRule = new ModelClientValidationRule();
            compareRule.ErrorMessage = this.ErrorMessageString;
            compareRule.ValidationType = "personalidentifiervalidation";

            compareRule.ValidationParameters.Add("propertyname", metadata.PropertyName);
            yield return compareRule;
        }
    }
}