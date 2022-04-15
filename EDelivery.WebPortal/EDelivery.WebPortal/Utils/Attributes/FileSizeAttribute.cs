using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class FileSizeAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly int maxFileSizeInMB;

        public FileSizeAttribute(int maxFileSizeMB)
        {
            this.maxFileSizeInMB = maxFileSizeMB;
        }

        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is HttpPostedFileBase)
            {
                HttpPostedFileBase file = value as HttpPostedFileBase;
                if (file.ContentLength >= (maxFileSizeInMB * 1024 * 1024))
                {
                    return new ValidationResult(
                        string.Format(this.ErrorMessageString, maxFileSizeInMB));
                }
            }

            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ModelMetadata metadata,
            ControllerContext context)
        {
            ModelClientValidationRule rule = new ModelClientValidationRule
            {
                ErrorMessage = string.Format(this.ErrorMessageString, maxFileSizeInMB),
                ValidationType = "filesize"
            };

            rule.ValidationParameters.Add("maxsize", maxFileSizeInMB * 1024 * 1024);

            yield return rule;
        }
    }
}