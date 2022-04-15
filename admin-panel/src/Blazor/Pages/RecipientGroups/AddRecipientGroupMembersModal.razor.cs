using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.RecipientGroups
{
    public class AddRecipientGroupMembersModalModel
    {
        public string[] ProfileIds { get; set; }
    }

    public partial class AddRecipientGroupMembersModal
    {
        [Parameter] public int RecipientGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddRecipientGroupMembersModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddRecipientGroupMembersModalModel model = new();

        private async Task SubmitFormAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.AddRecipientGroupMembersAsync(
                new AddRecipientGroupMembersRequest
                {
                    RecipientGroupId = this.RecipientGroupId,
                    ProfileIds =
                    {
                        this.model.ProfileIds
                            ?.Where(id => !string.IsNullOrEmpty(id))
                            ?.Select(id => int.Parse(id))
                            ?.ToArray()
                                ?? Array.Empty<int>()
                    },
                    ArchivedByAdminUserId = currentUserId
                });

            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
