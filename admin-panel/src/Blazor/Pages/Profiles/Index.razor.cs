using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class SearchProfilesModel
    {
        [Display(
            Name = nameof(IndexProfiles.LabelNameEmailPhone),
            ResourceType = typeof(IndexProfiles))]
        public string NameEmailPhone { get; set; }

        [Display(
            Name = nameof(IndexProfiles.LabelIdentifier),
            ResourceType = typeof(IndexProfiles))]
        public string Identifier { get; set; }
    }

    public partial class Index
    {
        [Inject] private IStringLocalizer<IndexProfiles> Localizer { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private EditContext editContext;

        private SearchProfilesModel model = new();

        private GetProfileListResponse profiles;

        private string recordsReportLink;

        protected override void OnInitialized()
        {
            this.model = new();

            this.editContext = new EditContext(this.model);

            base.OnInitialized();
        }

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();
            this.model.Identifier = this.NavigationManager.GetCurrentQueryItem<string>("identifier");
            this.model.NameEmailPhone = this.NavigationManager.GetCurrentQueryItem<string>("nameEmailPhone");
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.profiles =
                await this.AdminClient.GetProfileListAsync(
                    new GetProfileListRequest
                    {
                        AdminUserId = currentUserId,
                        Identifier = this.model.Identifier,
                        NameEmailPhone = this.model.NameEmailPhone,
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);

            this.recordsReportLink = $"Reports/ExportProfiles?identifier={this.model.Identifier}&nameEmailPhone={this.model.NameEmailPhone}";
        }

        private void Search()
        {
            this.NavigationManager.NavigateToSameWithQuery(
                new Dictionary<string, StringValues>
                {
                    { "page", 1.ToString() },
                    { "identifier", this.model.Identifier },
                    { "nameEmailPhone", this.model.NameEmailPhone },
                });
        }
    }
}
