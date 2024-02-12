using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models.Profile.Administration
{
    public class UpdateLoginProfilePermissionsViewModel
    {
        public UpdateLoginProfilePermissionsViewModel()
        {
        }

        public UpdateLoginProfilePermissionsViewModel(
            int loginId,
            int profileId,
            ED.DomainServices.Profiles.GetLoginPermissionsResponse loginPermissions,
            ED.DomainServices.Profiles.GetTemplatesResponse.Types.TemplateMessage[] templates)
        {
            this.LoginId = loginId;
            this.ProfileId = profileId;
            this.LoginName = loginPermissions.LoginName;
            this.ProfileIdentifier = loginPermissions.ProfileIdentifier;

            this.IsOwner = loginPermissions
                .Permissions
                .Any(e => e.Permission == ED.DomainServices.LoginProfilePermissionType.OwnerAccess);

            this.IsFullAccessMember = loginPermissions
                .Permissions
                .Any(e => e.Permission == ED.DomainServices.LoginProfilePermissionType.FullMessageAccess);

            this.IsRepresentative = loginPermissions
                .Permissions
                .Any(e => e.Permission == ED.DomainServices.LoginProfilePermissionType.ListProfileMessageAccess);

            this.IsAdmin = loginPermissions
                .Permissions
                .Any(e => e.Permission == ED.DomainServices.LoginProfilePermissionType.AdministerProfileAccess);

            this.Templates.AddRange(templates
                .Select(e => new UpdateLoginProfilePermissionsViewModelTemplates(
                    e.TemplateId,
                    e.Name,
                    loginPermissions
                        .Permissions
                        .Any(p => p.TemplateId == e.TemplateId && p.Permission == ED.DomainServices.LoginProfilePermissionType.ReadProfileMessageAccess),
                    loginPermissions
                        .Permissions
                        .Any(p => p.TemplateId == e.TemplateId && p.Permission == ED.DomainServices.LoginProfilePermissionType.WriteProfileMessageAccess))));
        }

        public int LoginId { get; set; }

        public int ProfileId { get; set; }

        public string LoginName { get; set; }

        public string ProfileIdentifier { get; set; }

        public bool IsOwner { get; set; }

        public bool IsRepresentative { get; set; }

        public bool IsFullAccessMember { get; set; }

        public bool IsAdmin { get; set; }

        public List<UpdateLoginProfilePermissionsViewModelTemplates> Templates { get; set; } =
            new List<UpdateLoginProfilePermissionsViewModelTemplates>();

        public List<ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage> ExtractPermissions()
        {
            List<ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage> permissions =
                new List<ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage>();

            if (this.IsAdmin)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.AdministerProfileAccess,
                        TemplateId = null
                    });
            }

            if (this.IsFullAccessMember)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.FullMessageAccess,
                        TemplateId = null
                    });
            }

            if (this.IsRepresentative)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.ListProfileMessageAccess,
                        TemplateId = null
                    });
            }

            foreach (UpdateLoginProfilePermissionsViewModelTemplates item in this.Templates)
            {
                if (item.HasReadPermission)
                {
                    permissions.Add(
                        new ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage
                        {
                            Permission = ED.DomainServices.LoginProfilePermissionType.ReadProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }

                if (item.HasWritePermission)
                {
                    permissions.Add(
                        new ED.DomainServices.Profiles.UpdateAccessRequest.Types.PermissionMessage
                        {
                            Permission = ED.DomainServices.LoginProfilePermissionType.WriteProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }
            }

            return permissions;
        }
    }

    public class UpdateLoginProfilePermissionsViewModelTemplates
    {
        public UpdateLoginProfilePermissionsViewModelTemplates()
        {
        }

        public UpdateLoginProfilePermissionsViewModelTemplates(
            int templateId,
            string templateName,
            bool hasReadPermission,
            bool hasWritePermission)
        {
            this.TemplateId = templateId;
            this.TemplateName = templateName;
            this.HasReadPermission = hasReadPermission;
            this.HasWritePermission = hasWritePermission;
        }

        public int TemplateId { get; set; }

        public string TemplateName { get; set; }

        public bool HasReadPermission { get; set; }

        public bool HasWritePermission { get; set; }
    }
}
