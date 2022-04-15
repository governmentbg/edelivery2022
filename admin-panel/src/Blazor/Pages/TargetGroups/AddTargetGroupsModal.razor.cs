using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.TargetGroups
{
    public class AddTargetGroupsModalModel
    {
        public string[] TargetGroupIds { get; set; }
    }

    public partial class AddTargetGroupsModal
    {
        [Parameter] public int TargetGroupId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddTargetGroupsModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddTargetGroupsModalModel model = new();

        private async Task SubmitFormAsync()
        {
            int currentUserId =
                await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            await this.AdminClient.InsertTargetGroupMatrixAsync(
                new InsertTargetGroupMatrixRequest
                {
                    TargetGroupId = this.TargetGroupId,
                    RecipientTargetGroupIds =
                    {
                        this.model.TargetGroupIds
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
