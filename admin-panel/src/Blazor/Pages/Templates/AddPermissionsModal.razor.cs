using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Templates
{
    public class AddPermissionsModalModel : IValidatableObject
    {
        public string[] ProfileIds { get; set; }

        public string[] TargetGroupIds { get; set; }

        public bool CanSend { get; set; }

        public bool CanReceive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((this.ProfileIds?.Length ?? 0) == 0 &&
                (this.TargetGroupIds?.Length ?? 0) == 0)
            {
                yield return new ValidationResult(
                    AddPermissionsModalResources.RequireProfileOrTargetGroupErrorMessage);
            }

            if (this.CanSend == false &&
                this.CanReceive == false)
            {
                yield return new ValidationResult(
                    AddPermissionsModalResources.RequireCanSendOrCanReceiveErrorMessage);
            }
        }
    }

    public partial class AddPermissionsModal
    {
        [Parameter] public int TemplateId { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddPermissionsModalResources> Localizer { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddPermissionsModalModel model = new();

        private async Task SubmitFormAsync()
        {
            _ = await this.AdminClient.CreateOrUpdateTemplatePermissionsAsync(
                new CreateOrUpdateTemplatePermissionsRequest
                {
                    TemplateId = this.TemplateId,
                    ProfileIds =
                    {
                        this.model.ProfileIds
                            ?.Where(id => !string.IsNullOrEmpty(id))
                            ?.Select(id => int.Parse(id))
                            ?.ToArray()
                            ?? Array.Empty<int>()
                    },
                    TargetGroupIds =
                    {
                        this.model.TargetGroupIds
                            ?.Where(id => !string.IsNullOrEmpty(id))
                            ?.Select(id => int.Parse(id))
                            ?.ToArray()
                            ?? Array.Empty<int>()
                    },
                    CanSend = this.model.CanSend,
                    CanReceive = this.model.CanReceive
                });

            await this.ModalInstance.CloseAsync();
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
