using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Requests
{
    public partial class Index
    {
        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<Index> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private GetRegistrationRequestListResponse requests;

        private Select2Option[] registrationRequestStatuses;

        private string DefaultStatus =>
            ((int)ED.DomainServices.RegistrationRequestStatus.None).ToString();

        private string status;

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();
            this.status =
                this.NavigationManager.GetCurrentQueryItem<string>("status")
                    ?? this.DefaultStatus;
        }

        private void Search()
        {
            var query = new Dictionary<string, StringValues>
            {
                { "page", 1.ToString() },
                { "status", this.status }
            };

            this.NavigationManager.NavigateToSameWithQuery(query);
        }

        protected override void OnInitialized()
        {
            this.registrationRequestStatuses = Enum
               .GetValues(typeof(ED.DomainServices.RegistrationRequestStatus))
               .Cast<ED.DomainServices.RegistrationRequestStatus>()
               .Select(e => new Select2Option
               {
                   Id = ((int)e).ToString(),
                   Text = this.Localizer[$"Status_{e}"],
               })
               .ToArray();
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.requests =
                await this.AdminClient.GetRegistrationRequestListAsync(
                    new GetRegistrationRequestListRequest
                    {
                        AdminUserId = currentUserId,
                        Status = this.status == this.DefaultStatus
                            ? null
                            : int.Parse(this.status),
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);
        }
    }
}
