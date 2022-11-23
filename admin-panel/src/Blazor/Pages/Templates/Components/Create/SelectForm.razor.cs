using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Pages.Templates.Components.Models;
using ED.AdminPanel.Blazor.Shared;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.AdminPanel.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Create
{
    public class SelectFormModel
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
    }

    public partial class SelectForm
    {
        [Inject] IStringLocalizer<FieldFormResources> Localizer { get; set; }

        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public SelectComponent Model { get; set; }

        public bool IsEdit { get => this.Model != null; }

        private SelectFormModel model;

        private ServerSideValidator serverSideValidator;

        private InputLargeTextArea TextArea;
        private long LastChangedLength { get; set; }

        public void TextAreaChanged(InputLargeTextAreaChangeEventArgs args)
        {
            this.LastChangedLength = args.Length;
        }

        protected override void OnInitialized()
        {
            this.model =
                this.Model == null
                    ? new()
                    : new(this.Model);

            this.LastChangedLength =
                Encoding.UTF8.GetByteCount(this.model.Options ?? string.Empty);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                if (this.TextArea != null)
                {
                    var memoryStream = new MemoryStream();
                    using var streamWriter = new StreamWriter(memoryStream);
                    await streamWriter.WriteAsync(this.model.Options);
                    await streamWriter.FlushAsync();
                    await this.TextArea.SetTextAsync(streamWriter);
                }
            }
        }

        private async Task SubmitFormAsync()
        {
            var streamReader = await this.TextArea!.GetTextAsync(maxLength: this.LastChangedLength);
            this.model.Options = await streamReader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(this.model.Options) &&
                string.IsNullOrWhiteSpace(this.model.Url))
            {
                this.serverSideValidator.ClearErrors();

                this.serverSideValidator.DisplayErrors(
                    new Dictionary<string, List<string>>
                    {
                        {
                            nameof(SelectFormModel.Options),
                            new List<string> { FieldFormResources.SelectUrlOrOptionsRequiredErrorMessage }
                        }
                    });
            }
            else
            {
                await this.ModalInstance.CloseAsync(
                    ModalResult.Ok(this.model.ToModel()));
            }
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
