using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using ED.AdminPanel.Blazor.Shared;
using ED.DomainServices;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class AddLoginModalModel : IValidatableObject
    {
        public bool FindIndividual { get; set; } = true;

        [Display(
            Name = nameof(AddLoginModalResources.FieldIdentifier),
            ResourceType = typeof(AddLoginModalResources))]
        public string Identifier { get; set; }

        public int? LoginId { get; set; }

        public string LoginElectronicSubjectName { get; set; }

        public bool IsRepresentative { get; set; }

        public bool IsFullAccessMember { get; set; }

        public bool IsAdmin { get; set; }

        public AddLoginModalModelTemplate[] Templates { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.FindIndividual)
            {
                foreach (var vr in this.RequiredValidation(m => m.Identifier))
                    yield return vr;
            }
        }
    }

    public class AddLoginModalModelTemplate
    {
        public int TemplateId { get; set; }

        public string TemplateName { get; set; }

        public bool HasReadPermission { get; set; }

        public bool HasWritePermission { get; set; }
    }

    public partial class AddLoginModal
    {
        [Parameter] public int ProfileId { get; set; }

        [Parameter] public int? LoginId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<AddLoginModalResources> Localizer { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        [CascadingParameter] private BlazoredModalInstance ModalInstance { get; set; }

        private AddLoginModalModel model;
        private ServerSideValidator serverSideValidator;

        protected override async Task OnInitializedAsync()
        {
            if (this.LoginId.HasValue)
            {
                GetProfileLoginPermissionsResponse loginPermissions =
                    await this.AdminClient.GetProfileLoginPermissionsAsync(
                        new GetProfileLoginPermissionsRequest
                        {
                            ProfileId = this.ProfileId,
                            LoginId = this.LoginId.Value,
                        });

                var allowedTemplates =
                    await this.AdminClient.GetProfileAccessAllowedTemplatesAsync(
                        new GetProfileAccessAllowedTemplatesRequest
                        {
                            ProfileId = this.ProfileId
                        });

                this.model = new()
                {
                    FindIndividual = false,
                    Identifier = loginPermissions.ProfileIdentifier,
                    LoginId = this.LoginId.Value,
                    LoginElectronicSubjectName = loginPermissions.LoginElectronicSubjectName,
                    IsRepresentative = loginPermissions.Permissions
                        .Any(lp => lp.Permission == LoginProfilePermissionType.ListProfileMessageAccess),
                    IsFullAccessMember = loginPermissions.Permissions
                        .Any(lp => lp.Permission == LoginProfilePermissionType.FullMessageAccess),
                    IsAdmin = loginPermissions.Permissions
                        .Any(lp => lp.Permission == LoginProfilePermissionType.AdministerProfileAccess),
                    Templates = allowedTemplates.Templates
                        .Select(t =>
                            new AddLoginModalModelTemplate
                            {
                                TemplateId = t.TemplateId,
                                TemplateName = t.Name,
                                HasReadPermission = loginPermissions.Permissions
                                    .Any(p => p.TemplateId == t.TemplateId &&
                                        p.Permission == LoginProfilePermissionType.ReadProfileMessageAccess),
                                HasWritePermission = loginPermissions.Permissions
                                    .Any(p => p.TemplateId == t.TemplateId &&
                                        p.Permission == LoginProfilePermissionType.WriteProfileMessageAccess)
                            })
                        .ToArray()
                };
            }
            else
            {
                this.model = new();
            }
        }

        private async Task SubmitFormAsync()
        {
            if (this.model.FindIndividual)
            {
                GetProfileAccessIndividualByIdentifierResponse resp =
                    await this.AdminClient.GetProfileAccessIndividualByIdentifierAsync(
                        new GetProfileAccessIndividualByIdentifierRequest
                        {
                            Identifier = this.model.Identifier,
                        });

                if (resp.Individual != null)
                {
                    this.model.LoginId = resp.Individual.LoginId;
                    this.model.LoginElectronicSubjectName = resp.Individual.LoginElectronicSubjectName;

                    var allowedTemplates =
                        await this.AdminClient.GetProfileAccessAllowedTemplatesAsync(
                            new GetProfileAccessAllowedTemplatesRequest
                            {
                                ProfileId = this.ProfileId
                            });

                    this.model.Templates = allowedTemplates.Templates
                        .Select(t =>
                            new AddLoginModalModelTemplate
                            {
                                TemplateId = t.TemplateId,
                                TemplateName = t.Name,
                                HasReadPermission = false,
                                HasWritePermission = false,
                            })
                        .ToArray();

                    this.model.FindIndividual = false;
                }
                else
                {
                    this.serverSideValidator.DisplayErrors(
                        new Dictionary<string, List<string>>
                        {
                            { nameof(AddLoginModalModel.Identifier),
                                new List<string> { AddLoginModalResources.PersonNotFoundError }}
                        });
                }
            }
            else
            {
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

                GrantOrUpdateProfileAccessRequest request = new()
                {
                    ProfileId = this.ProfileId,
                    LoginId = this.model.LoginId.Value,
                    AdminUserId = currentUserId,
                    Ip = this.ConnectionInfo.RemoteIpAddress,
                    Permissions = { this.ExtractPermissions() },
                };

                if (this.LoginId.HasValue)
                {
                    _ = await this.AdminClient.UpdateProfileAccessAsync(request);
                }
                else
                {
                    _ = await this.AdminClient.GrantProfileAccessAsync(request);
                }

                await this.ModalInstance.CloseAsync();
            }
        }

        private List<GrantOrUpdateProfileAccessRequest.Types.PermissionMessage> ExtractPermissions()
        {
            List<GrantOrUpdateProfileAccessRequest.Types.PermissionMessage> permissions = new();

            if (this.model.IsAdmin)
            {
                permissions.Add(
                    new GrantOrUpdateProfileAccessRequest.Types.PermissionMessage
                    {
                        Permission = LoginProfilePermissionType.AdministerProfileAccess,
                        TemplateId = null
                    });
            }

            if (this.model.IsFullAccessMember)
            {
                permissions.Add(
                    new GrantOrUpdateProfileAccessRequest.Types.PermissionMessage
                    {
                        Permission = LoginProfilePermissionType.FullMessageAccess,
                        TemplateId = null
                    });
            }

            if (this.model.IsRepresentative)
            {
                permissions.Add(
                    new GrantOrUpdateProfileAccessRequest.Types.PermissionMessage
                    {
                        Permission = LoginProfilePermissionType.ListProfileMessageAccess,
                        TemplateId = null
                    });
            }

            foreach (var item in this.model.Templates)
            {
                if (item.HasReadPermission)
                {
                    permissions.Add(
                        new GrantOrUpdateProfileAccessRequest.Types.PermissionMessage
                        {
                            Permission = LoginProfilePermissionType.ReadProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }

                if (item.HasWritePermission)
                {
                    permissions.Add(
                        new GrantOrUpdateProfileAccessRequest.Types.PermissionMessage
                        {
                            Permission = LoginProfilePermissionType.WriteProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }
            }

            return permissions;
        }

        private async Task CancelAsync()
        {
            await this.ModalInstance.CancelAsync();
        }
    }
}
