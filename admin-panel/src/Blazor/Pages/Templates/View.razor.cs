using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Pages.Templates.Components.Models;
using ED.AdminPanel.Blazor.Shared;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;

namespace ED.AdminPanel.Blazor.Pages.Templates
{
    public partial class View
    {
        [Parameter] public int TemplateId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }

        private GetTemplateResponse.Types.TemplateMessage template;
        private GetTemplatePermissionsResponse permissions;
        private IList<BaseComponent> content;

        private bool archiving;
        private bool publishing;
        private bool unpublishing;

        protected override async Task OnInitializedAsync()
        {
            await this.LoadTemplateAsync();
            await this.LoadPermissionAsync();
        }

        private async Task LoadTemplateAsync()
        {
            GetTemplateResponse resp =
               await this.AdminClient.GetTemplateAsync(
                   new GetTemplateRequest
                   {
                       TemplateId = this.TemplateId
                   });

            this.template = resp.Template;
            this.content = SerializationHelper.DeserializeModel(this.template.Content);
        }

        private async Task ArchiveAsync()
        {
            this.archiving = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.ArchiveTemplateAsync(
                new ArchiveTemplateRequest
                {
                    TemplateId = this.TemplateId,
                    ArchivedByAdminUserId = currentUserId
                });

            await this.LoadTemplateAsync();

            this.archiving = false;
        }

        private async Task PublishAsync()
        {
            this.publishing = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.PublishTemplateAsync(
                new PublishTemplateRequest
                {
                    TemplateId = this.TemplateId,
                    PublishedByAdminUserId = currentUserId
                });

            await this.LoadTemplateAsync();

            this.publishing = true;
        }

        private async Task UnpublishAsync()
        {
            this.unpublishing = true;

            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.UnpublishTemplateAsync(
                new UnpublishTemplateRequest
                {
                    TemplateId = this.TemplateId,
                    PublishedByAdminUserId = currentUserId
                });

            await this.LoadTemplateAsync();

            this.unpublishing = true;
        }

        private async Task AddPermissionsAsync()
        {
            ModalParameters parameters = new();
            parameters.Add(nameof(AddPermissionsModal.TemplateId), this.TemplateId);

            var componentModal = this.Modal.Show<AddPermissionsModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            if (!result.Cancelled)
            {
                await this.LoadPermissionAsync();
            }
        }

        private async Task DeleteProfilePermissionAsync(int profileId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmPermissionDelete"])))
            {
                return;
            }

            _ = await this.AdminClient.DeleteTemplateProfilePermissionAsync(
                new DeleteTemplateProfilePermissionRequest
                {
                    TemplateId = this.TemplateId,
                    ProfileId = profileId,
                });

            await this.LoadPermissionAsync();
        }

        private async Task DeleteTargetGroupPermissionAsync(int targetGroupId)
        {
            if (!(await this.Modal.ShowConfirmDangerModal(this.Localizer["MessageConfirmPermissionDelete"])))
            {
                return;
            }

            _ = await this.AdminClient.DeleteTemplateTargetGroupPermissionAsync(
                new DeleteTemplateTargetGroupPermissionRequest
                {
                    TemplateId = this.TemplateId,
                    TargetGroupId = targetGroupId,
                });

            // refresh the permissions
            await this.LoadPermissionAsync();
        }

        private async Task LoadPermissionAsync()
        {
            this.permissions =
                await this.AdminClient.GetTemplatePermissionsAsync(
                    new GetTemplatePermissionsRequest
                    {
                        TemplateId = this.TemplateId
                    });
        }
    }
}
