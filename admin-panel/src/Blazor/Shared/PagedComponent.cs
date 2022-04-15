using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace ED.AdminPanel.Blazor.Shared
{
    public abstract class PagedComponent : ComponentBase, IDisposable
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }

        protected bool IsLoading { get; private set; }

        protected int Page { get; private set; }

        protected int PageSize { get; set; } = 20;

        protected int Offset => (this.Page - 1) * this.PageSize;

        protected int Limit => this.PageSize;

        protected abstract Task LoadDataAsync(CancellationToken ct);

        private CancellationTokenSource cts = new();

        protected override async Task OnInitializedAsync()
        {
            this.NavigationManager.LocationChanged += this.OnLocationChanged;
            this.ExtractQueryStringParams();
            await this.LoadDataAsync(this.cts.Token);
            this.cts.Token.ThrowIfCancellationRequested();
        }

        protected virtual void ExtractQueryStringParams()
        {
            this.Page =
                this.NavigationManager.GetCurrentQueryItem<int?>("page")
                ?? 1;
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            this.ExtractQueryStringParams();

            this.IsLoading = true;
            this.StateHasChanged();

            base.InvokeAsync(async () =>
            {
                await this.LoadDataAsync(this.cts.Token);
                this.cts.Token.ThrowIfCancellationRequested();
                this.IsLoading = false;
                this.StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();
            this.NavigationManager.LocationChanged -= this.OnLocationChanged;
        }
    }
}
