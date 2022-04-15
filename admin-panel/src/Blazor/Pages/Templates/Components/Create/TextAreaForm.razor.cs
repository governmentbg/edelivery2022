using System;
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
    public class TextAreaFormModel
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

        public string Value { get; set; }

        public bool IsRequired { get; set; }

        public bool IsEncrypted { get; set; }

        public TextAreaFormModel()
        {
        }

        public TextAreaFormModel(TextAreaComponent model)
        {
            this.Label = model.Label;
            this.DocumentField = model.DocumentField;
            this.CustomClass = model.CustomClass;
            this.Placeholder = model.Placeholder;
            this.Value = model.Value;
            this.IsRequired = model.IsRequired;
            this.IsEncrypted = model.IsEncrypted;
        }

        public TextAreaComponent ToModel()
        {
            TextAreaComponent model = new()
            {
                Id = Guid.NewGuid(),
                Label = this.Label,
                DocumentField = this.DocumentField,
                CustomClass = this.CustomClass,
                Placeholder = this.Placeholder,
                Value = this.Value,
                IsRequired = this.IsRequired,
                IsEncrypted = this.IsEncrypted,
            };

            return model;
        }
    }

    public partial class TextAreaForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public TextAreaComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private TextAreaFormModel model;

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
