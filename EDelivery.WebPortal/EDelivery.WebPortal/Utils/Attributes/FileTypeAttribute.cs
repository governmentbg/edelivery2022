using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class FileTypeAttribute : ValidationAttribute, IClientValidatable
    {
        public string AllowExtensions { get; set; }

        public string DenyExtensions { get; set; }

        public FileTypeAttribute()
        { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            if (value is HttpPostedFileBase)
            {
                var file = value as HttpPostedFileBase;
                var ext = Path.GetExtension(file.FileName).ToLower().Trim('.');

                if (!string.IsNullOrEmpty(AllowExtensions))
                {
                    var list = AllowExtensions.ToLower().Split(',');
                    if (list.Contains(ext)) return ValidationResult.Success;
                    return new ValidationResult(this.ErrorMessageString);
                }

                if (!string.IsNullOrEmpty(DenyExtensions))
                {
                    var list = DenyExtensions.ToLower().Split(',');
                    if (!list.Contains(ext)) return ValidationResult.Success;
                    return new ValidationResult(this.ErrorMessageString);
                }
            }

            return ValidationResult.Success;
        }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule fileExtRule = new ModelClientValidationRule();
            fileExtRule.ErrorMessage = this.ErrorMessageString;
            fileExtRule.ValidationType = "filetype";

            fileExtRule.ValidationParameters.Add("allow", (AllowExtensions ?? "").ToLower());
            fileExtRule.ValidationParameters.Add("deny", (DenyExtensions ?? "").ToLower());
            yield return fileExtRule;
        }
    }
}