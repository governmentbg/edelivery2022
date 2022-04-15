using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ED.AdminPanel.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace ED.AdminPanel
{
    public static class ValidationExtensions
    {
        public static IEnumerable<ValidationResult> RequiredValidation<TInstance, TProp>(
            this TInstance instance,
            Expression<Func<TInstance, TProp>> propertyExpression)
            where TInstance : IValidatableObject
        {
            PropertyInfo propertyInfo = propertyExpression.GetPropertyAccess();
            object? value = propertyInfo.GetValue(instance);
            string propertyName = propertyInfo.Name;

            if (value == null ||
                ((value is string stringValue)
                    && string.IsNullOrWhiteSpace(stringValue)))
            {
                yield return new ValidationResult(
                    string.Format(
                        ErrorMessages.Required,
                        (new ValidationContext(instance) { MemberName = propertyName }).DisplayName),
                    new[] { propertyName });
            }
        }

        public static bool TryValidateObject(object instance)
        {
            ValidationContext context =
                new(instance, serviceProvider: null, items: null);
            List<ValidationResult> validationResults = new();

            return Validator.TryValidateObject(
                instance,
                context,
                validationResults,
                true);
        }
    }
}
