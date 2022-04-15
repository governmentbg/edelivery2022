using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Shared;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;

namespace ED.AdminPanel.Blazor.Pages.RecipientGroups
{
    public partial class View
    {
        [Parameter] public int RecipientGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }

        private GetRecipientGroupResponse.Types.RecipientGroupMessage recipientGroup;
        private GetRecipientGroupMembersResponse members;

        protected override async Task OnInitializedAsync()
        {
            GetRecipientGroupResponse resp =
               await this.AdminClient.GetRecipientGroupAsync(
                   new GetRecipientGroupRequest
                   {
                       RecipientGroupId = this.RecipientGroupId
                   });

            this.recipientGroup = resp.RecipientGroup;

            await this.LoadMembersAsync();
        }

        private async Task ArchiveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.ArchiveRecipientGroupAsync(
                new ArchiveRecipientGroupRequest
                {
                    RecipientGroupId = this.RecipientGroupId,
                    ArchivedByAdminUserId = currentUserId
                });

            this.NavigationManager.NavigateTo("recipient-groups");
        }

        private async Task AddRecipientGroupMembersAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(
                nameof(AddRecipientGroupMembersModal.RecipientGroupId),
                this.RecipientGroupId);

            var componentModal =
                this.Modal.Show<AddRecipientGroupMembersModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the members list
                await this.LoadMembersAsync();
            }
        }

        private async Task RemoveRecipientGroupMemberAsync(int profileId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmMemberDelete"])))
            {
                return;
            }

            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.RemoveRecipientGroupMembersAsync(
                new RemoveRecipientGroupMembersRequest
                {
                    RecipientGroupId = this.RecipientGroupId,
                    ProfileId = profileId,
                    ArchivedByAdminUserId = currentUserId
                });

            // refresh the members list
            await this.LoadMembersAsync();
        }

        private async Task LoadMembersAsync()
        {
            this.members =
                await this.AdminClient.GetRecipientGroupMembersAsync(
                    new GetRecipientGroupMembersRequest
                    {
                        RecipientGroupId = this.RecipientGroupId
                    });
        }
    }
}
