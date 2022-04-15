using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ED.AdminPanel.Utils
{
    public class DateTimeModelBinder : IModelBinder
    {
        public static readonly Type[] SupportedTypes = { typeof(DateTime), typeof(DateTime?) };

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (!SupportedTypes.Contains(bindingContext.ModelType))
            {
                return Task.CompletedTask;
            }

            string modelName = bindingContext.ModelName;

            ValueProviderResult valueProviderResult = bindingContext
                .ValueProvider
                .GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext
                .ModelState
                .SetModelValue(modelName, valueProviderResult);

            string dateTimeToParse = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(dateTimeToParse))
            {
                return Task.CompletedTask;
            }

            DateTime? dateTime = this.ParseDate(dateTimeToParse);

            bindingContext.Result = ModelBindingResult.Success(dateTime);

            return Task.CompletedTask;
        }

        private DateTime? ParseDate(string dateTimeToParse)
        {
            if (DateTime.TryParseExact(
                dateTimeToParse,
                Constants.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime validDate))
            {
                return validDate;
            }

            return null;
        }
    }

    public sealed class DateTimeModelBinderAttribute : ModelBinderAttribute
    {
        public DateTimeModelBinderAttribute()
            : base(typeof(DateTimeModelBinder))
        {
        }
    }
}
