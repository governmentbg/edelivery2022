using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Shared;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;

namespace ED.AdminPanel.Blazor.Pages.TargetGroups
{
    public partial class View
    {
        [Parameter] public int TargetGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }

        private GetTargetGroupResponse.Types.TargetGroupMessage targetGroup;
        private GetTargetGroupMatrixResponse matrix;

        protected override async Task OnInitializedAsync()
        {
            GetTargetGroupResponse resp =
               await this.AdminClient.GetTargetGroupAsync(
                   new GetTargetGroupRequest
                   {
                       TargetGroupId = this.TargetGroupId
                   });

            this.targetGroup = resp.TargetGroup;

            await this.LoadMaxtrixAsync();
        }

        private async Task ArchiveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.ArchiveTargetGroupAsync(
                new ArchiveTargetGroupRequest
                {
                    TargetGroupId = this.TargetGroupId,
                    ArchivedByAdminUserId = currentUserId
                });

            this.NavigationManager.NavigateTo("target-groups");
        }

        private async Task AddTargetGroupMatrixAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(
                nameof(AddTargetGroupsModal.TargetGroupId),
                this.TargetGroupId);

            var componentModal =
                this.Modal.Show<AddTargetGroupsModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the matrix
                await this.LoadMaxtrixAsync();
            }
        }

        private async Task RemoveTargetGroupMatrixAsync(int targetGroupId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmMemberDelete"])))
            {
                return;
            }

            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.DeleteTargetGroupMatrixAsync(
                new DeleteTargetGroupMatrixRequest
                {
                    TargetGroupId = this.TargetGroupId,
                    RecipientTargetGroupId = targetGroupId,
                    ArchivedByAdminUserId = currentUserId
                });

            // refresh the matrix
            await this.LoadMaxtrixAsync();
        }

        private async Task LoadMaxtrixAsync()
        {
            this.matrix =
                await this.AdminClient.GetTargetGroupMatrixAsync(
                    new GetTargetGroupMatrixRequest
                    {
                        TargetGroupId = this.TargetGroupId
                    });
        }
    }
}
