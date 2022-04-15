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
    public class SearchReceivedMessagesModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(ReceivedMessagesResources.FormFromDate),
            ResourceType = typeof(ReceivedMessagesResources))]
        public DateTime? FromDate { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(ReceivedMessagesResources.FormToDate),
            ResourceType = typeof(ReceivedMessagesResources))]
        public DateTime? ToDate { get; set; }

        [Display(
            Name = nameof(ReceivedMessagesResources.FormRecipientProfile),
            ResourceType = typeof(ReceivedMessagesResources))]
        public string RecipientProfileId { get; set; }

        [Display(
            Name = nameof(ReceivedMessagesResources.FormSenderProfile),
            ResourceType = typeof(ReceivedMessagesResources))]
        public string SenderProfileId { get; set; }

        public bool HasValues => this.FromDate.HasValue
            || this.ToDate.HasValue
            || !string.IsNullOrEmpty(this.RecipientProfileId)
            || !string.IsNullOrEmpty(this.SenderProfileId);

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
    }

    public partial class ReceivedMessages
    {
        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<ReceivedMessagesResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private EditContext editContext;

        private string messagesReportLink;
        private string messagesTableTitle;
        private GetReceivedMessageReportResponse messages;

        private SearchReceivedMessagesModel model = new();

        private bool hasSentRequest;

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
            this.model.RecipientProfileId = this.NavigationManager.GetCurrentQueryItem<string>("recipientProfileId");
            this.model.SenderProfileId = this.NavigationManager.GetCurrentQueryItem<string>("senderProfileId");
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

            this.messages =
                await this.AdminClient.GetReceivedMessageReportAsync(
                    new GetReceivedMessageReportRequest
                    {
                        AdminUserId = currentUserId,
                        FromDate = this.model.FromDate?.ToTimestamp(),
                        ToDate = this.model.ToDate?.ToTimestamp(),
                        RecipientProfileId = string.IsNullOrEmpty(this.model.RecipientProfileId)
                            ? null
                            : int.Parse(this.model.RecipientProfileId),
                        SenderProfileId = string.IsNullOrEmpty(this.model.SenderProfileId)
                            ? null
                            : int.Parse(this.model.SenderProfileId),
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);

            this.messagesReportLink = $"Reports/ExportReceivedMessages?fromDate={this.model.FromDate!.Value.ToString(Constants.DateTimeFormat)}&toDate={this.model.ToDate!.Value.ToString(Constants.DateTimeFormat)}&recipientProfileId={this.model.RecipientProfileId}&senderProfileId={this.model.SenderProfileId}&requestDate={this.requestDate.ToString(Constants.DateTimeFormat)}";
            this.messagesTableTitle = string.Format(
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
                    { "recipientProfileId", this.model.RecipientProfileId },
                    { "senderProfileId", this.model.SenderProfileId },
                    { "requestDate", DateTime.Now.ToQueryItem() }
                });
        }
    }
}
