using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Reports
{
    public class SearchTimestampsModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(TimestampsResources.FormFromDate),
            ResourceType = typeof(TimestampsResources))]
        public DateTime? FromDate { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(TimestampsResources.FormToDate),
            ResourceType = typeof(TimestampsResources))]
        public DateTime? ToDate { get; set; }

        public bool HasValues => this.FromDate.HasValue || this.ToDate.HasValue;

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
    }

    public partial class Timestamps
    {
        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<TimestampsResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private EditContext editContext;

        private string recordsReportLink;
        private string recordsTableTitle;
        private GetTimestampsReportResponse records;
        private SearchTimestampsModel model = new();

        private DateTime requestDate;

        protected override void OnInitialized()
        {
            this.model = new();

            this.editContext = new EditContext(this.model);
        }

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();

            this.model.FromDate = this.NavigationManager.GetQueryItemAsDateTime("fromDate");
            this.model.ToDate = this.NavigationManager.GetQueryItemAsDateTime("toDate");
            this.requestDate = this.NavigationManager.GetQueryItemAsDateTime("requestDate") ?? DateTime.Now;
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
                await this.AdminClient.GetTimestampsReportAsync(
                    new GetTimestampsReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = this.model.FromDate?.ToTimestamp(),
                        ToDate = this.model.ToDate?.ToTimestamp(),
                    },
                    cancellationToken: ct);

            this.recordsReportLink = $"Reports/ExportTimestamps?fromDate={this.model.FromDate!.Value.ToString(Constants.DateTimeFormat)}&toDate={this.model.ToDate!.Value.ToString(Constants.DateTimeFormat)}&requestDate={this.requestDate.ToString(Constants.DateTimeFormat)}";
            this.recordsTableTitle = string.Format(
                this.Localizer["ColumnAll"],
                this.model.FromDate.Value.ToString(Constants.DateTimeFormat),
                this.model.ToDate.Value.ToString(Constants.DateTimeFormat));
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
                    { "fromDate", this.model.FromDate.ToQueryItem() },
                    { "toDate", this.model.ToDate.ToQueryItem() },
                    { "requestDate", DateTime.Now.ToQueryItem() }
                });
        }
    }
}
