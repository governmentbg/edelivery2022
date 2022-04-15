using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profile
{
    public class EditModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormFirstName),
            ResourceType = typeof(EditResources))]
        public string FirstName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormMiddleName),
            ResourceType = typeof(EditResources))]
        public string MiddleName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormLastName),
            ResourceType = typeof(EditResources))]
        public string LastName { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormIdentifier),
            ResourceType = typeof(EditResources))]
        public string Identifier { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormPhone),
            ResourceType = typeof(EditResources))]
        [RegularExpression(
            Constants.PhoneRegex,
            ErrorMessageResourceName = nameof(ErrorMessages.ErrorInvalidPhone),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        public string Phone { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [EmailAddress(
             ErrorMessageResourceName = nameof(ErrorMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormEmail),
            ResourceType = typeof(EditResources))]
        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormOldPassword),
            ResourceType = typeof(EditResources))]
        public string OldPassword { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormPassword),
            ResourceType = typeof(EditResources))]
        public string Password { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Compare(
            nameof(ChangePasswordModel.Password),
            ErrorMessageResourceName = nameof(ErrorMessages.CompareError),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.FormConfirmPassword),
            ResourceType = typeof(EditResources))]
        public string ConfirmPassword { get; set; }
    }

    public partial class Edit
    {
        [Inject] private UserManager<IdentityUser<int>> userManager { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<EditResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private GetAdminProfileResponse administrator;
        private EditModel editModel;
        private ChangePasswordModel changePasswordModel;
        private ServerSideValidator changePasswordServerSideValidator;

        protected override async Task OnInitializedAsync()
        {
            this.editModel = new();
            this.changePasswordModel = new();

            await this.LoadAdminProfileAsync();

            this.editModel.FirstName = this.administrator.FirstName;
            this.editModel.MiddleName = this.administrator.MiddleName;
            this.editModel.LastName = this.administrator.LastName;
            this.editModel.Identifier = this.administrator.Identifier;
            this.editModel.Email = this.administrator.Email;
            this.editModel.Phone = this.administrator.Phone;
        }

        private async Task SaveAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.UpdateAdminProfileAsync(
                new UpdateAdminProfileRequest
                {
                    Id = currentUserId,
                    FirstName = this.editModel.FirstName,
                    MiddleName = this.editModel.MiddleName,
                    LastName = this.editModel.LastName,
                    Identifier = this.editModel.Identifier,
                    Phone = this.editModel.Phone,
                    Email = this.editModel.Email,
                });

            await this.LoadAdminProfileAsync();
        }

        private async Task ChangePasswordAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            ClaimsPrincipal claimsPrincipal =
                await this.AuthenticationStateHelper
                    .GetAuthenticatedUserClaimPrincipal();
            IdentityUser<int> user =
                await this.userManager.GetUserAsync(claimsPrincipal);

            if (await this.userManager.CheckPasswordAsync(user, this.changePasswordModel.OldPassword))
            {
                await this.AdminClient.ChangePasswordAdminProfileAsync(
                    new ChangePasswordAdminProfileRequest
                    {
                        Id = currentUserId,
                        PasswordHash = this.userManager.PasswordHasher.HashPassword(
                              null,
                              this.changePasswordModel.Password),
                    });

                this.changePasswordModel.OldPassword = string.Empty;
                this.changePasswordModel.Password = string.Empty;
                this.changePasswordModel.ConfirmPassword = string.Empty;
            }
            else
            {
                this.changePasswordServerSideValidator.ClearErrors();

                this.changePasswordServerSideValidator.DisplayErrors(
                    new Dictionary<string, List<string>>
                    {
                        {
                            nameof(ChangePasswordModel.OldPassword),
                            new List<string> { EditResources.ErrorOldPassword }
                        }
                    });
            }
        }

        private async Task LoadAdminProfileAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            this.administrator = await this.AdminClient.GetAdminProfileAsync(
                new GetAdminProfileRequest
                {
                    Id = currentUserId
                });
        }
    }
}
