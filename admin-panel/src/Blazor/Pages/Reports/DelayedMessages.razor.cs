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
    public class SearchDelayedMessagesModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(DelayedMessagesResources.FormDelay),
            ResourceType = typeof(DelayedMessagesResources))]
        public int? Delay { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(DelayedMessagesResources.FormTargetGroup),
            ResourceType = typeof(DelayedMessagesResources))]
        public string TargetGroupId { get; set; }

        [Display(
            Name = nameof(DelayedMessagesResources.FormProfile),
            ResourceType = typeof(DelayedMessagesResources))]
        public string ProfileId { get; set; }

        public bool HasValues => this.Delay.HasValue
            || !string.IsNullOrEmpty(this.TargetGroupId)
            || !string.IsNullOrEmpty(this.ProfileId);

        public bool IsValid => ValidationExtensions.TryValidateObject(this);
    }

    public partial class DelayedMessages
    {
        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<DelayedMessagesResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private EditContext editContext;

        private string profilesReportLink;
        private DateTime? fromDate;
        private GetDelayedMessagesReportResponse profiles;
        private SearchDelayedMessagesModel model = new();
         
        private bool hasSentRequest;

        protected override void OnInitialized()
        {
            this.model = new();

            this.editContext = new EditContext(this.model);
        }

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();

            this.model.Delay = this.NavigationManager.GetCurrentQueryItem<int?>("delay");
            this.model.TargetGroupId = this.NavigationManager.GetCurrentQueryItem<string>("targetGroupId");
            this.model.ProfileId = this.NavigationManager.GetCurrentQueryItem<string>("profileId");
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            if (!this.model.HasValues || !this.model.IsValid)
            {
                return;
            }

            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.profiles =
                await this.AdminClient.GetDelayedMessagesReportAsync(
                    new GetDelayedMessagesReportRequest
                    {
                        AdminUserId = currentUserId,
                        Delay = this.model.Delay.Value,
                        TargetGroupId = int.Parse(this.model.TargetGroupId),
                        ProfileId = string.IsNullOrEmpty(this.model.ProfileId)
                            ? null
                            : int.Parse(this.model.ProfileId),
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);

            this.fromDate = DateTime.Now.Date.AddDays(-this.model.Delay.Value);
            this.profilesReportLink = $"Reports/ExportDelayedMessages?delay={this.model.Delay}&targetGroupId={this.model.TargetGroupId}&profileId={this.model.ProfileId}&fromDate={this.fromDate!.Value.ToString(Constants.DateTimeFormat)}";

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
                    { "delay", this.model.Delay.ToString() },
                    { "targetGroupId", this.model.TargetGroupId },
                    { "profileId", this.model.ProfileId },
                });
        }
    }
}
