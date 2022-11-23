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
    public class SearchEFormsModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EFormsResources.FormFromDate),
            ResourceType = typeof(EFormsResources))]
        public DateTime? FromDate { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EFormsResources.FormToDate),
            ResourceType = typeof(EFormsResources))]
        public DateTime? ToDate { get; set; }

        [Display(
            Name = nameof(EFormsResources.FormSubject),
            ResourceType = typeof(EFormsResources))]
        public string Subject { get; set; }

        public bool HasValues =>
            this.FromDate.HasValue
            || this.ToDate.HasValue
            || !string.IsNullOrEmpty(this.Subject);

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
    }

    public partial class EForms
    {
        [Inject] private IStringLocalizer<EFormsResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private GrpcClientFactory GrpcClientFactory { get; set; }

        private EditContext editContext;

        private string recordsReportLink;
        private string recordsTableTitle;
        private GetEFormReportResponse records;

        private SearchEFormsModel model = new();

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

            this.model.FromDate = this.NavigationManager.GetQueryItemAsDateTime("fromDate");
            this.model.ToDate = this.NavigationManager.GetQueryItemAsDateTime("toDate");
            this.model.Subject = this.NavigationManager.GetCurrentQueryItem<string>("subject");
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
                await this.adminClient.GetEFormReportAsync(
                    new GetEFormReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = this.model.FromDate?.ToTimestamp(),
                        ToDate = this.model.ToDate?.ToTimestamp(),
                        Subject = this.model.Subject,
                    },
                    cancellationToken: ct);

            this.recordsReportLink = $"Reports/ExportEForms?fromDate={this.model.FromDate!.Value.ToString(Constants.DateTimeFormat)}&toDate={this.model.ToDate!.Value.ToString(Constants.DateTimeFormat)}&subject={this.model.Subject}";
            this.recordsTableTitle = string.Format(
                this.Localizer["ColumnAll"],
                this.model.FromDate.Value.ToString(Constants.DateTimeFormat),
                this.model.ToDate.Value.ToString(Constants.DateTimeFormat));

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
                    { "fromDate", this.model.FromDate.ToQueryItem() },
                    { "toDate", this.model.ToDate.ToQueryItem() },
                    { "subject", this.model.Subject }
                });
        }
    }
}
