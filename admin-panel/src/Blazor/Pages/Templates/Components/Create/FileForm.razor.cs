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
    public class FileFormModel
    {
        public const int MinInstances = 1;
        public const int MaxInstances = 10;

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(FieldFormResources.Label),
            ResourceType = typeof(FieldFormResources))]
        public string Label { get; set; }

        public string DocumentField { get; set; }

        public string CustomClass { get; set; }

        public string AllowedExtensions { get; set; }

        public int MaxSize { get; set; } = 512;

        public int ExpirationPeriod { get; set; } = 36;

        public bool IsRequired { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(FieldFormResources.Label),
            ResourceType = typeof(FieldFormResources))]
        [Range(
            MinInstances,
            MaxInstances,
            ErrorMessageResourceName = nameof(ErrorMessages.RangeMinMax),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        public int Instances { get; set; }

        public FileFormModel()
        {
        }

        public FileFormModel(FileComponent model)
        {
            this.Label = model.Label;
            this.DocumentField = model.DocumentField;
            this.CustomClass = model.CustomClass;
            this.AllowedExtensions = model.AllowedExtensions;
            this.MaxSize = model.MaxSize;
            this.ExpirationPeriod = model.ExpirationPeriod;
            this.IsRequired = model.IsRequired;
            this.Instances = model.Instances;
        }

        public FileComponent ToModel()
        {
            FileComponent model = new()
            {
                Id = Guid.NewGuid(),
                Label = this.Label,
                DocumentField = this.DocumentField,
                CustomClass = this.CustomClass,
                AllowedExtensions = this.AllowedExtensions,
                MaxSize = this.MaxSize,
                ExpirationPeriod = this.ExpirationPeriod,
                IsRequired = this.IsRequired,
                Instances = this.Instances,
            };

            return model;
        }
    }

    public partial class FileForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public FileComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private FileFormModel model;

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
