using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class EditNotificationsModalModel
    {
        [Display(
            Name = nameof(EditNotificationsModalResources.FormEmail),
            ResourceType = typeof(EditNotificationsModalResources))]
        public string Email { get; set; }

        [Required(
           ErrorMessageResourceName = nameof(ErrorMessages.Required),
           ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
           Name = nameof(EditNotificationsModalResources.FormEmailNotificationActive),
           ResourceType = typeof(EditNotificationsModalResources))]
        public bool EmailNotificationActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditNotificationsModalResources.FormEmailNotificationOnDeliveryActive),
            ResourceType = typeof(EditNotificationsModalResources))]
        public bool EmailNotificationOnDeliveryActive { get; set; }

        [Display(
            Name = nameof(EditNotificationsModalResources.FormPhone),
            ResourceType = typeof(EditNotificationsModalResources))]
        public string Phone { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditNotificationsModalResources.FormSmsNotificationActive),
            ResourceType = typeof(EditNotificationsModalResources))]
        public bool SmsNotificationActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditNotificationsModalResources.FormSmsNotificationOnDeliveryActive),
            ResourceType = typeof(EditNotificationsModalResources))]
        public bool SmsNotificationOnDeliveryActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditNotificationsModalResources.FormViberNotificationActive),
            ResourceType = typeof(EditNotificationsModalResources))]
        public bool ViberNotificationActive { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditNotificationsModalResources.FormViberNotificationOnDeliveryActive),
            ResourceType = typeof(EditNotificationsModalResources))]
        public bool ViberNotificationOnDeliveryActive { get; set; }
    }

    public partial class EditNotificationsModal
    {
        [Parameter] public int ProfileId { get; set; }

        [Parameter] public int LoginId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<EditNotificationsModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private EditNotificationsModalModel model;

        protected override async Task OnInitializedAsync()
        {
            GetLoginProfileNotificationsResponse notifications =
                await this.AdminClient.GetLoginProfileNotificationsAsync(
                    new GetLoginProfileNotificationsRequest
                    {
                        LoginId = this.LoginId,
                        ProfileId = this.ProfileId,
                    });

            this.model = new()
            {
                Email = notifications.Email,
                Phone = notifications.Phone,
                EmailNotificationActive = notifications.EmailNotificationActive,
                EmailNotificationOnDeliveryActive = notifications.EmailNotificationOnDeliveryActive,
                SmsNotificationActive = notifications.SmsNotificationActive,
                SmsNotificationOnDeliveryActive = notifications.SmsNotificationOnDeliveryActive,
                ViberNotificationActive = notifications.ViberNotificationActive,
                ViberNotificationOnDeliveryActive = notifications.ViberNotificationOnDeliveryActive,
            };
        }

        private async Task SubmitFormAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            _ = await this.AdminClient.UpdateLoginProfileNotificationsAsync(
                new UpdateLoginProfileNotificationsRequest
                {
                    ProfileId = this.ProfileId,
                    LoginId = this.LoginId,
                    Email = this.model.Email,
                    Phone = this.model.Phone,
                    EmailNotificationActive = this.model.EmailNotificationActive,
                    EmailNotificationOnDeliveryActive = this.model.EmailNotificationOnDeliveryActive,
                    SmsNotificationActive = this.model.SmsNotificationActive,
                    SmsNotificationOnDeliveryActive = this.model.SmsNotificationOnDeliveryActive,
                    ViberNotificationActive = this.model.ViberNotificationActive,
                    ViberNotificationOnDeliveryActive = this.model.ViberNotificationOnDeliveryActive,
                    AdminUserId = currentUserId,
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
