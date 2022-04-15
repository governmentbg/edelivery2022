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
    public class HiddenFormModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(FieldFormResources.Label),
            ResourceType = typeof(FieldFormResources))]
        public string Label { get; set; }

        public string DocumentField { get; set; }

        public string Value { get; set; }

        public HiddenFormModel()
        {
        }

        public HiddenFormModel(HiddenComponent model)
        {
            this.Label = model.Label;
            this.DocumentField = model.DocumentField;
            this.Value = model.Value;
        }

        public HiddenComponent ToModel()
        {
            HiddenComponent model = new()
            {
                Id = Guid.NewGuid(),
                Label = this.Label,
                DocumentField = this.DocumentField,
                Value = this.Value,
            };

            return model;
        }
    }

    public partial class HiddenForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public HiddenComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private HiddenFormModel model;

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
