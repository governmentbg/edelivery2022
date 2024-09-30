using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class AddIntegrationLoginModalModel
    {
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormCertificateThumbPrint),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public string CertificateThumbPrint { get; set; }

        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormPushNotificationsUrl),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public string PushNotificationsUrl { get; set; }

        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormCanSendOnBehalfOf),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public bool CanSendOnBehalfOf { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormPhoneNotificationActive),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public bool PhoneNotificationActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormPhoneNotificationOnDeliveryActive),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public bool PhoneNotificationOnDeliveryActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormEmailNotificationActive),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public bool EmailNotificationActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormEmailNotificationOnDeliveryActive),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public bool EmailNotificationOnDeliveryActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormEmail),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(AddIntegrationLoginModalResources.FormPhone),
            ResourceType = typeof(AddIntegrationLoginModalResources))]
        public string Phone { get; set; }
    }

    public partial class AddIntegrationLoginModal
    {
        [Parameter] public int ProfileId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddIntegrationLoginModalResources> Localizer { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddIntegrationLoginModalModel model;

        protected override async Task OnInitializedAsync()
        {
            GetIntegrationLoginInfoResponse integrationLogin =
                await this.AdminClient.GetIntegrationLoginInfoAsync(
                    new GetIntegrationLoginInfoRequest
                    {
                        ProfileId = this.ProfileId,
                    });

            this.model = new()
            {
                CertificateThumbPrint = integrationLogin.Login?.CertificateThumbPrint,
                PushNotificationsUrl = integrationLogin.Login?.PushNotificationsUrl,
                CanSendOnBehalfOf = integrationLogin.Login?.CanSendOnBehalfOf ?? false,
                PhoneNotificationActive = integrationLogin.Login?.PhoneNotificationActive ?? false,
                PhoneNotificationOnDeliveryActive = integrationLogin.Login?.PhoneNotificationOnDeliveryActive ?? false,
                EmailNotificationActive = integrationLogin.Login?.EmailNotificationActive ?? false,
                EmailNotificationOnDeliveryActive = integrationLogin.Login?.EmailNotificationOnDeliveryActive ?? false,
                Email = integrationLogin.Login?.Email,
                Phone = integrationLogin.Login?.Phone,
            };
        }

        private async Task SaveFormAsync()
        {
            _ = await this.AdminClient.CreateOrUpdateIntegrationLoginAsync(
                new CreateOrUpdateIntegrationLoginRequest
                {
                    ProfileId = this.ProfileId,
                    CanSendOnBehalfOf = this.model.CanSendOnBehalfOf,
                    CertificateThumbPrint = this.model.CertificateThumbPrint ?? string.Empty,
                    PushNotificationsUrl = this.model.PushNotificationsUrl ?? string.Empty,
                    Email = this.model.Email,
                    EmailNotificationActive = this.model.EmailNotificationActive,
                    EmailNotificationOnDeliveryActive = this.model.EmailNotificationOnDeliveryActive,
                    Phone = this.model.Phone,
                    PhoneNotificationActive = this.model.PhoneNotificationActive,
                    PhoneNotificationOnDeliveryActive = this.model.PhoneNotificationOnDeliveryActive,
                });

            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
