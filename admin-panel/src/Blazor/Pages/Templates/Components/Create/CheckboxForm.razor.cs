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
    public class CheckboxFormModel
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

        public bool Value { get; set; }

        public bool IsEncrypted { get; set; }

        public CheckboxFormModel()
        {
        }

        public CheckboxFormModel(CheckboxComponent model)
        {
            this.Label = model.Label;
            this.DocumentField = model.DocumentField;
            this.CustomClass = model.CustomClass;
            this.Value = model.Value;
            this.IsEncrypted = model.IsEncrypted;
        }

        public CheckboxComponent ToModel()
        {
            CheckboxComponent model = new()
            {
                Id = Guid.NewGuid(),
                Label = this.Label,
                DocumentField = this.DocumentField,
                CustomClass = this.CustomClass,
                Value = this.Value,
                IsEncrypted = this.IsEncrypted,
            };

            return model;
        }
    }

    public partial class CheckboxForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public CheckboxComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private CheckboxFormModel model;

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
