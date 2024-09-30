using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models.Profile.Administration
{
    public class AddLoginProfilePermissionsViewModel
    {
        public AddLoginProfilePermissionsViewModel()
        {
        }

        public AddLoginProfilePermissionsViewModel(
            int loginId,
            int profileId,
            string loginName,
            string profileIdentifier,
            bool isEmailNotificationEnabled,
            bool isEmailNotificationOnDeliveryEnabled,
            bool isPhoneNotificationEnabled,
            bool isPhoneNotificationOnDeliveryEnabled,
            ED.DomainServices.Profiles.GetTemplatesResponse.Types.TemplateMessage[] templates)
        {
            this.LoginId = loginId;
            this.LoginName = loginName;
            this.ProfileIdentifier = profileIdentifier;
            this.IsEmailNotificationEnabled = isEmailNotificationEnabled;
            this.IsEmailNotificationOnDeliveryEnabled = isEmailNotificationOnDeliveryEnabled;
            this.IsPhoneNotificationEnabled = isPhoneNotificationEnabled;
            this.IsPhoneNotificationOnDeliveryEnabled = isPhoneNotificationOnDeliveryEnabled;

            this.Templates.AddRange(templates
               .Select(e => new AddLoginProfilePermissionsViewModelTemplates(
                   e.TemplateId,
                   e.Name,
                   false,
                   false)));
        }

        public int LoginId { get; set; }

        public int ProfileId { get; set; }

        public string LoginName { get; set; }

        public string ProfileIdentifier { get; set; }

        public bool IsEmailNotificationEnabled { get; set; }

        public bool IsEmailNotificationOnDeliveryEnabled { get; set; }

        public bool IsPhoneNotificationEnabled { get; set; }

        public bool IsPhoneNotificationOnDeliveryEnabled { get; set; }

        public bool IsRepresentative { get; set; }

        public bool IsFullAccessMember { get; set; }

        public bool IsAdmin { get; set; }

        public List<AddLoginProfilePermissionsViewModelTemplates> Templates { get; set; } =
            new List<AddLoginProfilePermissionsViewModelTemplates>();

        public List<ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage> ExtractPermissions()
        {
            List<ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage> permissions =
                new List<ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage>();

            if (this.IsAdmin)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.AdministerProfileAccess,
                        TemplateId = null
                    });
            }

            if (this.IsFullAccessMember)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.FullMessageAccess,
                        TemplateId = null
                    });
            }

            if (this.IsRepresentative)
            {
                permissions.Add(
                    new ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage
                    {
                        Permission = ED.DomainServices.LoginProfilePermissionType.ListProfileMessageAccess,
                        TemplateId = null
                    });
            }

            foreach (AddLoginProfilePermissionsViewModelTemplates item in this.Templates)
            {
                if (item.HasReadPermission)
                {
                    permissions.Add(
                        new ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage
                        {
                            Permission = ED.DomainServices.LoginProfilePermissionType.ReadProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }

                if (item.HasWritePermission)
                {
                    permissions.Add(
                        new ED.DomainServices.Profiles.GrantAccessRequest.Types.PermissionMessage
                        {
                            Permission = ED.DomainServices.LoginProfilePermissionType.WriteProfileMessageAccess,
                            TemplateId = item.TemplateId
                        });
                }
            }

            return permissions;
        }
    }

    public class AddLoginProfilePermissionsViewModelTemplates
    {
        public AddLoginProfilePermissionsViewModelTemplates()
        {
        }

        public AddLoginProfilePermissionsViewModelTemplates(
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
