using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class AddProfileQuotasModalModel
    {
        public const int MinStorageQuotaInMb = 0;
        public const int MaxStorageQuotaInMb = 1024 * 1024;

        [Range(
            MinStorageQuotaInMb,
            MaxStorageQuotaInMb,
            ErrorMessageResourceName = nameof(ErrorMessages.RangeMinMax),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddProfileQuotasModalResources.FormStorageQuotaInMb),
            ResourceType = typeof(AddProfileQuotasModalResources))]
        public int? StorageQuotaInMb { get; set; }
    }

    public partial class AddProfileQuotasModal
    {
        [Parameter] public int ProfileId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddProfileQuotasModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddProfileQuotasModalModel model;

        protected override async Task OnInitializedAsync()
        {
            GetProfileQuotasInfoResponse profileQuotasInfo =
                await this.AdminClient.GetProfileQuotasInfoAsync(
                    new GetProfileQuotasInfoRequest
                    {
                        ProfileId = this.ProfileId,
                    });

            this.model = new()
            {
                StorageQuotaInMb = profileQuotasInfo.StorageQuotaInMb
            };
        }

        private async Task SaveFormAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.UpdateProfileQuotasAsync(
                new UpdateProfileQuotasRequest
                {
                    AdminUserId = currentUserId,
                    ProfileId = this.ProfileId,
                    StorageQuotaInMb = this.model.StorageQuotaInMb,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
