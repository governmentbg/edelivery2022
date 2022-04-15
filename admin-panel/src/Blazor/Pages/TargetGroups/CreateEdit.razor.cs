using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.TargetGroups
{
    public class CreateEditModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.Name),
            ResourceType = typeof(CreateEditResources))]
        public string Name { get; set; }
    }

    public partial class CreateEdit
    {
        [Parameter] public int? EditTargetGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<CreateEditResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private CreateEditModel model;

        protected override async Task OnInitializedAsync()
        {
            this.model = new();

            if (this.EditTargetGroupId != null)
            {
                GetTargetGroupResponse resp =
                    await this.AdminClient.GetTargetGroupAsync(
                        new GetTargetGroupRequest
                        {
                            TargetGroupId = this.EditTargetGroupId.Value
                        });

                this.model.Name = resp.TargetGroup.Name;
            }
        }

        private async Task SaveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            if (this.EditTargetGroupId == null)
            {

                CreateTargetGroupResponse response =
                    await this.AdminClient.CreateTargetGroupAsync(
                        new CreateTargetGroupRequest
                        {
                            Name = this.model.Name,
                            AdminUserId = currentUserId,
                        });

                this.NavigationManager.NavigateTo(
                    $"target-groups/{response.TargetGroupId}");
            }
            else
            {
                await this.AdminClient.EditTargetGroupAsync(
                    new EditTargetGroupRequest
                    {
                        TargetGroupId = this.EditTargetGroupId.Value,
                        Name = this.model.Name,
                        AdminUserId = currentUserId,
                    });

                this.NavigationManager.NavigateTo(
                    $"target-groups/{this.EditTargetGroupId.Value}");
            }
        }
    }
}
