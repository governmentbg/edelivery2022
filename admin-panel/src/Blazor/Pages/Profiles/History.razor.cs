using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class SearchProfileHistoryModel
    {
        public string[] Actions { get; set; }
    }

    public partial class History
    {
        [Inject] private IStringLocalizer<HistoryResources> Localizer { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        private EditContext editContext;

        [Parameter] public int ProfileId { get; set; }

        private SearchProfileHistoryModel model = new();
        private GetProfileHistoryResponse history;
        private Select2Option[] actions;

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();

            this.model.Actions = this.NavigationManager.GetCurrentQueryItem<string[]>("actions");
        }

        protected override void OnInitialized()
        {
            this.actions = new Select2Option[]
            {
                new Select2Option { Text = DomainServices.ProfileHistoryAction.CreateProfile.ToString(), Id = DomainServices.ProfileHistoryAction.CreateProfile.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.AccessProfile.ToString(), Id = DomainServices.ProfileHistoryAction.AccessProfile.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.ProfileActivated.ToString(), Id = DomainServices.ProfileHistoryAction.ProfileActivated.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.ProfileDeactivated.ToString(), Id = DomainServices.ProfileHistoryAction.ProfileDeactivated.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.GrantAccessToProfile.ToString(), Id = DomainServices.ProfileHistoryAction.GrantAccessToProfile.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.RemoveAccessToProfile.ToString(), Id = DomainServices.ProfileHistoryAction.RemoveAccessToProfile.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.ProfileUpdated.ToString(), Id = DomainServices.ProfileHistoryAction.ProfileUpdated.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.BringProfileInForce.ToString(), Id = DomainServices.ProfileHistoryAction.BringProfileInForce.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.MarkAsReadonly.ToString(), Id = DomainServices.ProfileHistoryAction.MarkAsReadonly.ToString() },
                new Select2Option { Text = DomainServices.ProfileHistoryAction.MarkAsNonReadonly.ToString(), Id = DomainServices.ProfileHistoryAction.MarkAsNonReadonly.ToString() },
            };

            this.model = new();

            this.editContext = new EditContext(this.model);

            base.OnInitialized();
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            DomainServices.ProfileHistoryAction[] actions = this.model.Actions?
                .Select(e => Enum.Parse<DomainServices.ProfileHistoryAction>(e))
                .ToArray()
                    ?? Array.Empty<DomainServices.ProfileHistoryAction>();

            this.history =
                await this.AdminClient.GetProfileHistoryAsync(
                    new GetProfileHistoryRequest
                    {
                        ProfileId = this.ProfileId,
                        Actions = { actions },
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);
        }

        private void Search()
        {
            this.NavigationManager.NavigateToSameWithQuery(
                new Dictionary<string, StringValues>
                {
                    { "page", 1.ToString() },
                    { "actions", this.model.Actions }
                });

            //List<KeyValuePair<string, StringValues>> kvp = new()
            //{
            //    new KeyValuePair<string, StringValues>("page", 1.ToString())
            //};

            //if (this.model.Actions != null)
            //{
            //    kvp.AddRange(
            //        this.model.Actions?
            //            .Select(e => new KeyValuePair<string, StringValues>("actions", e)));
            //}

            //this.NavigationManager.NavigateToSameWithQuery(kvp);
        }
    }
}
