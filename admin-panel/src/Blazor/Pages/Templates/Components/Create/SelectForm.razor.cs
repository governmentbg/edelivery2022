using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Pages.Templates.Components.Models;
using ED.AdminPanel.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Create
{
    public class SelectFormModel : IValidatableObject
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(FieldFormResources.Label),
            ResourceType = typeof(FieldFormResources))]
        public string Label { get; set; }

        public string DocumentField { get; set; }

        public string CustomClass { get; set; }

        public string Placeholder { get; set; }

        public string Url { get; set; }

        public string Options { get; set; }

        public string Value { get; set; }

        public bool IsRequired { get; set; }

        public bool IsEncrypted { get; set; }

        public SelectFormModel()
        {
        }

        public SelectFormModel(SelectComponent model)
        {
            this.Label = model.Label;
            this.DocumentField = model.DocumentField;
            this.CustomClass = model.CustomClass;
            this.Placeholder = model.Placeholder;
            this.Url = model.Url;
            this.Options = model.Options;
            this.Value = model.Value;
            this.IsRequired = model.IsRequired;
            this.IsEncrypted = model.IsEncrypted;
        }

        public SelectComponent ToModel()
        {
            SelectComponent model = new()
            {
                Id = Guid.NewGuid(),
                Label = this.Label,
                DocumentField = this.DocumentField,
                CustomClass = this.CustomClass,
                Placeholder = this.Placeholder,
                Url = this.Url,
                Options = this.Options,
                Value = this.Value,
                IsRequired = this.IsRequired,
                IsEncrypted = this.IsEncrypted,
            };

            return model;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(this.Options) &&
                string.IsNullOrWhiteSpace(this.Url))
            {
                yield return new ValidationResult(
                    FieldFormResources.SelectUrlOrOptionsRequiredErrorMessage,
                    new[] { nameof(this.Options) });
            }
        }
    }

    public partial class SelectForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public SelectComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private SelectFormModel model;

        protected override void OnParametersSet()
        {
            this.model =
                this.Model == null
                    ? new()
                    : new(this.Model);
        }

        private async Task SubmitFormAsync()
        {
            await this.ModalInstance.CloseAsync(
                ModalResult.Ok(
                    this.model.ToModel()));
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
