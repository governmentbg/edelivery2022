using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.RecipientGroups
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
        [Parameter] public int? EditRecipientGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<CreateEditResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private CreateEditModel model;

        protected override async Task OnInitializedAsync()
        {
            this.model = new();

            if (this.EditRecipientGroupId != null)
            {
                GetRecipientGroupResponse resp =
                    await this.AdminClient.GetRecipientGroupAsync(
                        new GetRecipientGroupRequest
                        {
                            RecipientGroupId = this.EditRecipientGroupId.Value
                        });

                this.model.Name = resp.RecipientGroup.Name;
            }
        }

        private async Task SaveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            if (this.EditRecipientGroupId == null)
            {

                CreateRecipientGroupResponse resp =
                    await this.AdminClient.CreateRecipientGroupAsync(
                        new CreateRecipientGroupRequest
                        {
                            Name = this.model.Name,
                            AdminUserId = currentUserId,
                        });

                this.NavigationManager.NavigateTo(
                    $"recipient-groups/{resp.RecipientGroupId}");
            }
            else
            {
                await this.AdminClient.EditRecipientGroupAsync(
                    new EditRecipientGroupRequest
                    {
                        RecipientGroupId = this.EditRecipientGroupId.Value,
                        Name = this.model.Name,
                        AdminUserId = currentUserId,
                    });

                this.NavigationManager.NavigateTo(
                    $"recipient-groups/{this.EditRecipientGroupId.Value}");
            }
        }
    }
}
