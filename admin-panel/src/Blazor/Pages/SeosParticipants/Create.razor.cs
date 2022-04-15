using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.SeosParticipants
{
    public class CreateModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormServiceAddress),
            ResourceType = typeof(CreateResources))]
        public string ServiceAddress { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormIdentifier),
            ResourceType = typeof(CreateResources))]
        public string Identifier { get; set; }
    }

    public partial class Create
    {
        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<CreateResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        private CreateModel model;

        protected override void OnInitialized()
        {
            this.model = new();
        }

        private async Task SaveAsync()
        {
            _ = await this.AdminClient.CreateSeosParticipantAsync(
                new CreateSeosParticipantRequest()
                {
                    As4Node = this.model.ServiceAddress,
                    RegisteredEntityIdentifier = this.model.Identifier
                });

            this.NavigationManager.NavigateTo($"seos-participants/");
        }
    }
}

