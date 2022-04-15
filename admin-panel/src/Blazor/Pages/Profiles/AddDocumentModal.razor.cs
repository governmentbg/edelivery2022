using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class AddDocumentModalModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddDocumentModalResources.BlobId),
            ResourceType = typeof(AddDocumentModalResources))]
        public BlobValue BlobValue { get; set; }

        [StringLength(
            maximumLength: 500,
            ErrorMessageResourceName = nameof(ErrorMessages.StringLengthMax),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddDocumentModalResources.Description),
            ResourceType = typeof(AddDocumentModalResources))]
        public string Description { get; set; }
    }

    public partial class AddDocumentModal
    {
        [Parameter] public int ProfileId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddDocumentModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddDocumentModalModel model = new();

        private async Task SubmitFormAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
            await this.AdminClient.AddProfileRegistrationDocumentAsync(
                new AddProfileRegistrationDocumentRequest
                {
                    ProfileId = this.ProfileId,
                    AdminUserId = currentUserId,
                    BlobId = this.model.BlobValue.BlobId
                });
            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
