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
    public class SearchSentMessagesModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(SentMessagesResources.FormFromDate),
            ResourceType = typeof(SentMessagesResources))]
        public DateTime? FromDate { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(SentMessagesResources.FormToDate),
            ResourceType = typeof(SentMessagesResources))]
        public DateTime? ToDate { get; set; }

        [Display(
            Name = nameof(SentMessagesResources.FormRecipientProfile),
            ResourceType = typeof(SentMessagesResources))]
        public string RecipientProfileId { get; set; }

        [Display(
            Name = nameof(SentMessagesResources.FormSenderProfile),
            ResourceType = typeof(SentMessagesResources))]
        public string SenderProfileId { get; set; }

        public bool HasValues => this.FromDate.HasValue
            || this.ToDate.HasValue
            || !string.IsNullOrEmpty(this.RecipientProfileId)
            || !string.IsNullOrEmpty(this.SenderProfileId);

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
    }

    public partial class SentMessages
    {
        [Inject] private IStringLocalizer<SentMessagesResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private GrpcClientFactory GrpcClientFactory { get; set; }

        private EditContext editContext;

        private string messagesReportLink;
        private string messagesTableTitle;
        private GetSentMessageReportResponse messages;
        private SearchSentMessagesModel model = new();

        private bool hasSentRequest;

        private DateTime requestDate;

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
                await this.adminClient.GetSentMessageReportAsync(
                    new GetSentMessageReportRequest
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

            this.messagesReportLink = $"Reports/ExportSentMessages?fromDate={this.model.FromDate!.Value.ToString(Constants.DateTimeFormat)}&toDate={this.model.ToDate!.Value.ToString(Constants.DateTimeFormat)}&recipientProfileId={this.model.RecipientProfileId}&senderProfileId={this.model.SenderProfileId}&requestDate={this.requestDate.ToString(Constants.DateTimeFormat)}";
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
