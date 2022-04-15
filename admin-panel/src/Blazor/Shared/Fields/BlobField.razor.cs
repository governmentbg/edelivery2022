using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA5394 // Do not use insecure randomness

namespace ED.AdminPanel.Blazor.Shared.Fields
{
    public enum FileType
    {
        NotSpecified = 0,
        Template = 1,
        Registration = 2,
    }

    public enum MalwareScanStatus
    {
        None = 1,
        NotMalicious,
        NotSure,
        IsMalicious
    }

    public record BlobDO(
        string Name,
        long Size,
        int? BlobId,
        MalwareScanStatus MalwareScanStatus)
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MalwareScanStatus MalwareScanStatus { get; init; } =
            MalwareScanStatus;
    }

    public record BlobValue(
        string Name,
        int BlobId);

    public partial class BlobField : IDisposable, IAsyncDisposable
    {
        [Inject] private IOptions<AdminPanelOptions> Options { get; set; }
        [Inject] private IWebHostEnvironment Environment { get; set; }
        [Inject] private IStringLocalizer<BlobField> Localizer { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }

        [CascadingParameter] private EditContext CascadedEditContext { get; set; }

        [Parameter] public BlobValue Value { get; set; }

        [Parameter] public EventCallback<BlobValue> ValueChanged { get; set; }

        [Parameter] public Expression<Func<BlobValue>> ValueExpression { get; set; }

        [Parameter] public FileType Type { get; set; }

        [Parameter] public int? MaxFileSizeBytes { get; set; }

        [Parameter] public IEnumerable<string> AllowedFileTypes { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        protected string FieldCssClass
            => this.CascadedEditContext?.FieldCssClass(this.fieldIdentifier) ?? "";

        private ValidationMessageStore validationMessages;
        private bool hasValidationMessages;
        private DotNetObjectReference<BlobField> objectReference;
        private IJSObjectReference moduleReference;
        private string id;
        private FieldIdentifier fieldIdentifier;
        private string fileName;
        private bool uploading;

        protected override void OnInitialized()
        {
            if (this.Type == FileType.NotSpecified)
            {
                throw new Exception($"Missing required parameter {nameof(this.Type)}");
            }

            if (string.IsNullOrEmpty(this.AdditionalAttributes.GetValueOrDefault("id") as string))
            {
                // we need an id to identify the select
                this.id = $"file-field-{(new Random()).Next()}";
                this.AdditionalAttributes.AddOrUpdate("id", this.id);
            }
            else
            {
                this.id = (string)this.AdditionalAttributes["id"];
            }

            if (this.ValueExpression != null && this.CascadedEditContext != null)
            {
                this.fieldIdentifier = FieldIdentifier.Create(this.ValueExpression);
                this.validationMessages = new ValidationMessageStore(this.CascadedEditContext);
            }

            if (this.Value != null)
            {
                this.fileName = this.Value.Name;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.moduleReference =
                    await this.JSRuntime.InvokeAsync<IJSObjectReference>(
                        "import",
                        "./scripts/uppy.js");

                string blobServiceUrl = this.Options.Value.BlobServiceWebUrl;
                string path = this.Type switch
                {
                    FileType.Template => "system/template",
                    FileType.Registration => "system/registration",
                    _ => throw new Exception("Unsupported file type"),
                };

                this.objectReference = DotNetObjectReference.Create(this);
                await this.moduleReference.InvokeVoidAsync(
                    "create",
                    $"#{this.id}",
                    this.objectReference,
                    new
                    {
                        endpoint = new Uri(new Uri(blobServiceUrl), path).ToString(),
                        debug = this.Environment.IsDevelopment(),
                        maxFileSizeBytes = this.MaxFileSizeBytes,
                        allowedFileTypes = this.AllowedFileTypes
                    });
            }
        }

        [JSInvokable("add")]
        public async Task AddAsync(string fileName)
        {
            this.uploading = true;
            this.fileName = fileName;

            if (this.hasValidationMessages)
            {
                this.validationMessages?.Clear(this.fieldIdentifier);
                this.hasValidationMessages = false;
                this.CascadedEditContext?.NotifyValidationStateChanged();
            }

            if (this.Value != null)
            {
                this.Value = null;
                await this.ValueChanged.InvokeAsync(this.Value);
                this.CascadedEditContext?.NotifyFieldChanged(this.fieldIdentifier);
            }

            this.StateHasChanged();
        }

        [JSInvokable("error")]
        public Task ErrorAsync(string error)
        {
            this.uploading = false;

            this.validationMessages?.Add(this.fieldIdentifier, this.GetErrorMessage(error));
            this.hasValidationMessages = true;
            this.CascadedEditContext?.NotifyValidationStateChanged();

            this.StateHasChanged();

            return Task.CompletedTask;
        }

        [JSInvokable("success")]
        public async Task SuccessAsync(BlobDO blob)
        {
            this.uploading = false;

            if (this.hasValidationMessages)
            {
                this.validationMessages?.Clear(this.fieldIdentifier);
                this.hasValidationMessages = false;
                this.CascadedEditContext?.NotifyValidationStateChanged();
            }

            if (blob.BlobId != null)
            {
                BlobValue newValue = new(blob.Name, blob.BlobId.Value);
                if (this.Value != newValue)
                {
                    this.Value = newValue;
                    await this.ValueChanged.InvokeAsync(this.Value);
                    this.CascadedEditContext?.NotifyFieldChanged(this.fieldIdentifier);
                }
            }

            this.StateHasChanged();
        }

        public async Task ClearAsync()
        {
            this.fileName = null;

            if (this.hasValidationMessages)
            {
                this.validationMessages?.Clear(this.fieldIdentifier);
                this.hasValidationMessages = false;
                this.CascadedEditContext?.NotifyValidationStateChanged();
            }

            this.Value = null;
            await this.ValueChanged.InvokeAsync(this.Value);
            this.CascadedEditContext?.NotifyFieldChanged(this.fieldIdentifier);
        }

        public async ValueTask DisposeAsync()
        {
            if (this.moduleReference != null)
            {
                await this.moduleReference.InvokeVoidAsync("destroy", $"#{this.id}");
                await this.moduleReference.DisposeAsync();
            }
        }

        public void Dispose()
        {
            if (this.objectReference != null)
            {
                this.objectReference.Dispose();
                this.objectReference = null;
            }
        }

        private string GetErrorMessage(string error)
            => error switch
            {
                "fileSizeExceeded" =>
                    string.Format(
                        this.Localizer["FileSizeExceededErrorMessage"],
                        ((double)this.MaxFileSizeBytes / (1024 * 1024)).ToString("0.#")),
                "extensionNotAllowed" =>
                    string.Format(
                        this.Localizer["ExtensionNotAllowedErrorMessage"],
                        string.Join(", ", this.AllowedFileTypes)),
                "unknown" => this.Localizer["UnknownErrorMessage"],
                _ => this.Localizer[error]
            };
    }
}
