using System;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Shared;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public partial class View
    {
        [Parameter] public int ProfileId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private IStringLocalizer<View> Localizer { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private BlobUrlCreator BlobUrlCreator { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }

        private GetProfileInfoResponse profile;
        private bool activating;
        private bool deactivating;

        private bool profileReadonlyActivating;
        private bool profileReadonlyDeactivating;

        private bool loginActivating;
        private bool loginDeactivating;

        protected override async Task OnParametersSetAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
            await this.LoadProfileAsync(currentUserId);

            await base.OnParametersSetAsync();
        }

        private async Task ActivateAsync()
        {
            if (!this.profile.CanBeActivated)
            {
                throw new Exception("This profile cannot be activated");
            }

            this.activating = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.ActivateProfileAsync(
                new ActivateProfileRequest
                {
                    AdminUserId = currentUserId,
                    ProfileId = this.ProfileId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            await this.LoadProfileAsync(currentUserId);

            this.activating = false;
        }

        private async Task DeactivateAsync()
        {
            this.deactivating = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.DeactivateProfileAsync(
                new DeactivateProfileRequest
                {
                    AdminUserId = currentUserId,
                    ProfileId = this.ProfileId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            await this.LoadProfileAsync(currentUserId);

            this.deactivating = false;
        }

        private async Task MarkProfileAsNonReadonlyAsync()
        {
            this.profileReadonlyDeactivating = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.MarkProfileAsNonReadonlyAsync(
                new MarkProfileAsNonReadonlyRequest
                {
                    AdminUserId = currentUserId,
                    ProfileId = this.ProfileId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            await this.LoadProfileAsync(currentUserId);

            this.profileReadonlyDeactivating = false;
        }

        private async Task MarkProfileAsReadonlyAsync()
        {
            this.profileReadonlyActivating = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.MarkProfileAsReadonlyAsync(
                new MarkProfileAsReadonlyRequest
                {
                    AdminUserId = currentUserId,
                    ProfileId = this.ProfileId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            await this.LoadProfileAsync(currentUserId);

            this.profileReadonlyActivating = false;
        }

        private async Task ActivateLoginAsync(int loginId)
        {
            this.loginActivating = true;

            _ = await this.AdminClient.ActivateLoginAsync(
                new ActivateLoginRequest
                {
                    LoginId = loginId,
                });

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.LoadProfileAsync(currentUserId);

            this.loginActivating = false;
        }

        private async Task DeactivateLoginAsync(int loginId)
        {
            this.loginDeactivating = true;

            _ = await this.AdminClient.DeactivateLoginAsync(
                new DeactivateLoginRequest()
                {
                    LoginId = loginId,
                });

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.LoadProfileAsync(currentUserId);

            this.loginDeactivating = false;
        }

        private async Task AddDocumentAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddDocumentModal.ProfileId), this.ProfileId);

            var componentModal = this.Modal.Show<AddDocumentModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task RemoveDocumentAsync(int blobId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmDocumentDelete"])))
            {
                return;
            }

            await this.AdminClient.RemoveProfileRegistrationDocumentAsync(
                new RemoveProfileRegistrationDocumentRequest()
                {
                    ProfileId = this.ProfileId,
                    BlobId = blobId
                });

            // refresh the page
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
            await this.LoadProfileAsync(currentUserId);
        }

        private async Task AddLoginAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddLoginModal.ProfileId), this.ProfileId);

            var componentModal = this.Modal.Show<AddLoginModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task ViewLoginAsync(int loginId)
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddLoginModal.ProfileId), this.ProfileId);
            parameters.Add(nameof(AddLoginModal.LoginId), loginId);

            var componentModal = this.Modal.Show<AddLoginModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task RemoveLoginAsync(int loginId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmLoginDelete"])))
            {
                return;
            }

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.RevokeProfileAccessAsync(
                new RevokeProfileAccessRequest
                {
                    ProfileId = this.ProfileId,
                    LoginId = loginId,
                    AdminUserId = currentUserId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                });

            // refresh the page
            await this.LoadProfileAsync(currentUserId);
        }

        private async Task AddIntegrationLoginAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddIntegrationLoginModal.ProfileId), this.ProfileId);

            var componentModal = this.Modal.Show<AddIntegrationLoginModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task AddProfileQuotasAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddProfileQuotasModal.ProfileId), this.ProfileId);

            var componentModal = this.Modal.Show<AddProfileQuotasModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task AddProfileEsbUserAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddProfileEsbUserModal.ProfileId), this.ProfileId);

            var componentModal = this.Modal.Show<AddProfileEsbUserModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task ViewNotificationsAsync(int loginId)
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(EditNotificationsModal.ProfileId), this.ProfileId);
            parameters.Add(nameof(EditNotificationsModal.LoginId), loginId);

            var componentModal = this.Modal.Show<EditNotificationsModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                // refresh the page
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
                await this.LoadProfileAsync(currentUserId);
            }
        }

        private async Task LoadProfileAsync(int currentUserId)
        {
            this.profile = await this.AdminClient.GetProfileInfoAsync(
                new GetProfileInfoRequest
                {
                    ProfileId = this.ProfileId,
                    AdminUserId = currentUserId,
                });
        }
    }
}
