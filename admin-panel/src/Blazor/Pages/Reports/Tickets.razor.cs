using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Reports
{
    public class SearchTicketsModel : IValidatableObject
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(TicketsResources.FormFrom),
            ResourceType = typeof(TicketsResources))]
        public DateTime? From { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(TicketsResources.FormTo),
            ResourceType = typeof(TicketsResources))]
        public DateTime? To { get; set; }

        public bool HasValues => this.From.HasValue || this.To.HasValue;

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

    public partial class Tickets
    {
        [Inject] private IStringLocalizer<TicketsResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private GrpcClientFactory GrpcClientFactory { get; set; }

        private EditContext editContext;

        private string recordsReportLink;
        private string recordsTableTitle;
        private GetTicketsReportResponse records;
        private SearchTicketsModel model = new();

        private bool hasSentRequest;

        private Admin.AdminClient adminClient;

        protected override void OnInitialized()
        {
            this.model = new();

            this.editContext = new EditContext(this.model);

            this.adminClient = this.GrpcClientFactory.CreateClient<Admin.AdminClient>(Startup.GrpcReportsClient);
        }

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();

            this.model.From = this.NavigationManager.GetQueryItemAsDateTime("from");
            this.model.To = this.NavigationManager.GetQueryItemAsDateTime("to");
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            if (!this.model.HasValues || !this.model.IsValid)
            {
                return;
            }

            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.records =
                await this.adminClient.GetTicketsReportAsync(
                    new GetTicketsReportRequest()
                    {
                        AdminUserId = currentUserId,
                        From = this.model.From!.Value.ToTimestamp(),
                        To = this.model.To!.Value.ToTimestamp(),
                    },
                    cancellationToken: ct);

            this.recordsReportLink = $"Reports/ExportTickets?from={this.model.From!.Value.ToString(Constants.DateTimeFormat)}&to={this.model.To!.Value.ToString(Constants.DateTimeFormat)}";
            this.recordsTableTitle = string.Format(
                this.Localizer["ColumnAll"],
                this.model.From!.Value.ToString(Constants.DateTimeFormat),
                this.model.To!.Value.ToString(Constants.DateTimeFormat));

            this.hasSentRequest = true;
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && this.model.HasValues)
            {
                this.editContext.Validate();
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private void Search()
        {
            this.NavigationManager.NavigateToSameWithQuery(
                new Dictionary<string, StringValues>
                {
                    { "page", 1.ToString() },
                    { "from", this.model.From!.Value.ToQueryItem() },
                    { "to", this.model.To!.Value.ToQueryItem() },
                });
        }
    }
}
