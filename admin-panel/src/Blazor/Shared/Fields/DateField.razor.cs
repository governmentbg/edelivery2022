using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

#pragma warning disable CA2227 // Collection properties should be read only

namespace ED.AdminPanel.Blazor.Shared.Fields
{
    public sealed partial class DateField : IAsyncDisposable
    {
        [Inject] IJSRuntime JSRuntime { get; set; }

        [CascadingParameter] private EditContext CascadedEditContext { get; set; }

        [Parameter] public DateTime? Value { get; set; }
        [Parameter] public EventCallback<DateTime?> ValueChanged { get; set; }
        [Parameter] public Expression<Func<DateTime?>> ValueExpression { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        [Parameter] public string Placeholder { get; set; }

        private string FieldCssClass
            => this.CascadedEditContext?.FieldCssClass(this.fieldIdentifier) ?? "";

        private IJSObjectReference moduleReference;
        private string id;
        private FieldIdentifier fieldIdentifier;

        protected override void OnInitialized()
        {
            this.fieldIdentifier = FieldIdentifier.Create(this.ValueExpression!);
        }

        protected override void OnParametersSet()
        {
            if (string.IsNullOrEmpty(this.AdditionalAttributes.GetValueOrDefault("id") as string))
            {
#pragma warning disable CA5394 // Do not use insecure randomness
                // we need an id to identify the select
                this.id = $"select2-{(new Random()).Next()}";
                this.AdditionalAttributes.AddOrUpdate("id", this.id);
#pragma warning restore CA5394 // Do not use insecure randomness
            }
            else
            {
                this.id = (string)this.AdditionalAttributes["id"];
            }

            if (string.IsNullOrEmpty(this.Placeholder))
            {
                this.Placeholder = "";
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                string datepickerLanguage =
                    CultureInfo.CurrentCulture.Equals(new CultureInfo("en-US"))
                        ? "en"
                        : "bg";

                this.moduleReference =
                    await this.JSRuntime.InvokeAsync<IJSObjectReference>(
                        "import",
                        "./scripts/datepicker.js");

                await this.moduleReference.InvokeVoidAsync(
                    "create",
                    $"#{this.id}",
                    new
                    {
                        language = datepickerLanguage,
                        format = "dd-mm-yyyy"
                    },
                    this.Value?.ToString(Constants.DateTimeFormat) ?? string.Empty);
            }
        }

        public async Task OnChangeAsync(ChangeEventArgs args)
        {
            string valueStr = args.Value.ToString();
            this.Value = null;
            if (!string.IsNullOrEmpty(valueStr))
            {
                this.Value = DateTime.ParseExact(valueStr, Constants.DateTimeFormat, null);
            }
            await this.ValueChanged.InvokeAsync(this.Value);
            this.CascadedEditContext?.NotifyFieldChanged(this.fieldIdentifier);
        }

        private async Task ClearAsync()
        {
            this.Value = null;
            await this.ValueChanged.InvokeAsync(this.Value);
            await this.moduleReference!.InvokeVoidAsync(
                "clear",
                $"#{this.id}");
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (this.moduleReference != null)
            {
                await this.moduleReference.InvokeVoidAsync("destroy", $"#{this.id}");
                await this.moduleReference.DisposeAsync();
            }
        }
    }
}
