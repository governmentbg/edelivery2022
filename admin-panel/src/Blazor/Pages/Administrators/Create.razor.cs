using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Administrators
{
    public class CreateModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormFirstName),
            ResourceType = typeof(CreateResources))]
        public string FirstName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormMiddleName),
            ResourceType = typeof(CreateResources))]
        public string MiddleName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormLastName),
            ResourceType = typeof(CreateResources))]
        public string LastName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormIdentifier),
            ResourceType = typeof(CreateResources))]
        public string Identifier { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormPhone),
            ResourceType = typeof(CreateResources))]
        public string Phone { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [EmailAddress(
             ErrorMessageResourceName = nameof(ErrorMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormEmail),
            ResourceType = typeof(CreateResources))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormUserName),
            ResourceType = typeof(CreateResources))]
        public string UserName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormPassword),
            ResourceType = typeof(CreateResources))]
        public string Password { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Compare(
            nameof(CreateModel.Password),
            ErrorMessageResourceName = nameof(ErrorMessages.CompareError),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateResources.FormConfirmPassword),
            ResourceType = typeof(CreateResources))]
        public string ConfirmPassword { get; set; }
    }

    public partial class Create
    {
        [Inject] private UserManager<IdentityUser<int>> userManager { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<CreateResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private CreateModel model;
        private ServerSideValidator serverSideValidator;

        protected override void OnInitialized()
        {
            this.model = new();
        }

        private async Task SaveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            CreateAdministratorResponse resp =
                   await this.AdminClient.CreateAdministratorAsync(
                       new CreateAdministratorRequest
                       {
                           FirstName = this.model.FirstName,
                           MiddleName = this.model.MiddleName,
                           LastName = this.model.LastName,
                           Identifier = this.model.Identifier,
                           Phone = this.model.Phone,
                           Email = this.model.Email,
                           UserName = this.model.UserName,
                           PasswordHash = this.userManager.PasswordHasher.HashPassword(
                               null,
                               this.model.Password),
                           AdminUserId = currentUserId,
                       });

            if (resp.IsSuccessful)
            {
                this.NavigationManager.NavigateTo($"administrators/{resp.Id}");
            }
            else
            {
                this.serverSideValidator.ClearErrors();

                this.serverSideValidator.DisplayErrors(
                    new Dictionary<string, List<string>>
                    {
                        {
                            nameof(CreateModel.UserName),
                            new List<string> { resp.Error }
                        }
                    });
            }
        }
    }
}
