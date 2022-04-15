using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace ED.AdminPanel.Blazor.Shared.Fields
{
    public sealed class Select2Option
    {
        public string Id { get; set; }

        public string Text { get; set; }
    }

    public abstract class Select2FieldBase<T> : ComponentBase, IDisposable, IAsyncDisposable
    {
        [Inject] IJSRuntime JSRuntime { get; set; }

        [CascadingParameter] private EditContext CascadedEditContext { get; set; }

        [Parameter] public T Value { get; set; }
        [Parameter] public EventCallback<T> ValueChanged { get; set; }
        [Parameter] public Expression<Func<T>> ValueExpression { get; set; }

        [Parameter] public string Placeholder { get; set; }

        [Parameter] public IEnumerable<Select2Option> Options { get; set; }

        [Parameter] public string Url { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

        protected string FieldCssClass
            => this.CascadedEditContext?.FieldCssClass(this.fieldIdentifier) ?? "";

        private DotNetObjectReference<Select2FieldBase<T>> objectReference;
        private IJSObjectReference moduleReference;
        private string id;
        private FieldIdentifier fieldIdentifier;

        protected override void OnInitialized()
        {
            this.fieldIdentifier = FieldIdentifier.Create(this.ValueExpression!);
        }

        protected override void OnParametersSet()
        {
            if (this.Options == null && this.Url == null)
            {
                throw new Exception($"Either {nameof(this.Options)} or {nameof(this.Url)} should be set");
            }

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
                // required to allow clearing
                this.Placeholder = " ";
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                string select2Language =
                    CultureInfo.CurrentCulture.Equals(new CultureInfo("en-US"))
                        ? "en"
                        : "bg";

                this.moduleReference =
                    await this.JSRuntime.InvokeAsync<IJSObjectReference>(
                        "import",
                        "./scripts/select2.js");

                this.objectReference = DotNetObjectReference.Create(this);
                await this.moduleReference.InvokeVoidAsync(
                    "create",
                    $"#{this.id}",
                    this.Options != null
                        ? new
                        {
                            language = select2Language,
                            allowClear = true,
                            placeholder = this.Placeholder,
                            data = this.Options
                                ?.Select(o =>
                                    new
                                    {
                                        id = o.Id,
                                        text = o.Text,
                                    }),
                        }
                        : new
                        {
                            language = select2Language,
                            allowClear = true,
                            placeholder = this.Placeholder,
                            ajax =
                                new
                                {
                                    url = this.Url,
                                    dataType = "json",
                                    delay = 250,
                                },
                        },
                    this.Value,
                    new[] { "change" },
                    this.objectReference);
            }
        }

        [JSInvokable("change")]
        public async Task ChangeAsync()
        {
            var selected =
                await this.moduleReference!.InvokeAsync<Select2Option[]>(
                    "getData",
                    $"#{this.id}");

            await this.NotifyValueChangedAsync(selected);
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

        protected abstract Task NotifyValueChangedAsync(Select2Option[] selected);
    }
}
