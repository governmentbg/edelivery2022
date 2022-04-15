using System;
using System.Threading.Tasks;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Administrators
{
    public partial class View
    {
        [Parameter] public int Id { get; set; }

        [Inject] private IStringLocalizer<View> Localizer { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private GetAdministratorResponse administrator;
        private int currentUserId;
        private bool activating;
        private bool deactivating;

        protected override async Task OnInitializedAsync()
        {
            this.currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.LoadAdministratorAsync();
        }

        private async Task ActivateAsync()
        {
            if (this.administrator.IsActive)
            {
                throw new Exception("This administrator cannot be activated");
            }

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            if (currentUserId == this.Id)
            {
                throw new Exception("This administrator cannot activate its own account");
            }

            this.activating = true;

            await this.AdminClient.ActivateAdministratorAsync(
                new ActivateAdministratorRequest
                {
                    Id = this.Id,
                });

            await this.LoadAdministratorAsync();

            this.activating = false;
        }

        private async Task DeactivateAsync()
        {
            if (!this.administrator.IsActive)
            {
                throw new Exception("This administrator cannot be deactivated");
            }

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            if (currentUserId == this.Id)
            {
                throw new Exception("This administrator cannot deactivate its own account");
            }

            this.deactivating = true;

            await this.AdminClient.DeactivateAdministratorAsync(
                new DeactivateAdministratorRequest
                {
                    AdminUserId = currentUserId,
                    Id = this.Id,
                });

            await this.LoadAdministratorAsync();

            this.deactivating = false;
        }

        private async Task LoadAdministratorAsync()
        {
            this.administrator = await this.AdminClient.GetAdministratorAsync(
                new GetAdministratorRequest
                {
                    Id = this.Id
                });
        }
    }
}
