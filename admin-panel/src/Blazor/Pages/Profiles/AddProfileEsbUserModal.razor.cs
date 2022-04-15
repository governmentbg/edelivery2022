using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class AddProfileEsbUserModalModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddProfileEsbUserModalResources.FormServiceOId),
            ResourceType = typeof(AddProfileEsbUserModalResources))]
        public string OId { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddProfileEsbUserModalResources.FormClientId),
            ResourceType = typeof(AddProfileEsbUserModalResources))]
        public string ClientId { get; set; }
    }

    public partial class AddProfileEsbUserModal
    {
        [Parameter] public int ProfileId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddProfileEsbUserModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddProfileEsbUserModalModel model;

        protected override async Task OnInitializedAsync()
        {
            GetProfileEsbUserInfoResponse profileEsbUser =
                await this.AdminClient.GetProfileEsbUserInfoAsync(
                    new GetProfileEsbUserInfoRequest
                    {
                        ProfileId = this.ProfileId,
                    });

            this.model = new()
            {
                OId = profileEsbUser.OId,
                ClientId = profileEsbUser.ClientId,
            };
        }

        private async Task SaveFormAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.UpdateProfileEsbUserAsync(
                new UpdateProfileEsbUserRequest
                {
                    ProfileId = this.ProfileId,
                    OId = this.model.OId,
                    ClientId = this.model.ClientId,
                    AdminUserId = currentUserId,
                });

            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
