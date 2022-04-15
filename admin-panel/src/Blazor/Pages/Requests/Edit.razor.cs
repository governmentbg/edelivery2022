using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Requests
{
    public class EditModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.Comment),
            ResourceType = typeof(EditResources))]
        public string Comment { get; set; }
    }

    public partial class Edit
    {
        [Parameter] public int RegistrationRequestId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<EditResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private BlobUrlCreator BlobUrlCreator { get; set; }

        private EditContext editContext;

        private GetRegistrationRequestResponse request;

        private EditModel model;

        private ServerSideValidator serverSideValidator;

        protected override async Task OnInitializedAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.model = new();

            this.editContext = new EditContext(this.model);

            this.request = await this.AdminClient.GetRegistrationRequestAsync(
                new GetRegistrationRequestRequest
                {
                    AdminUserId = currentUserId,
                    RegistrationRequestId = RegistrationRequestId
                });
        }

        private async Task RejectAsync()
        {
            if (!this.editContext.Validate())
            {
                return;
            }

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.RejectRegistrationRequestAsync(
                new RejectRegistrationRequestRequest
                {
                    Comment = this.model.Comment,
                    AdminUserId = currentUserId,
                    RegistrationRequestId = this.RegistrationRequestId
                });

            this.NavigationManager.NavigateTo("requests");
        }

        private async Task SaveAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            ConfirmRegistrationRequestResponse resp = await this.AdminClient.ConfirmRegistrationRequestAsync(
                new ConfirmRegistrationRequestRequest
                {
                    Comment = this.model.Comment,
                    AdminUserId = currentUserId,
                    RegistrationRequestId = this.RegistrationRequestId
                });

            if (resp.IsSuccessful)
            {
                this.NavigationManager.NavigateTo("requests");
            }
            else
            {
                this.serverSideValidator.ClearErrors();

                this.serverSideValidator.DisplayErrors(
                    new Dictionary<string, List<string>>
                    {
                        {
                            nameof(EditModel.Comment),
                            new List<string> { resp.Error }
                        }
                    });
            }
        }
    }
}
