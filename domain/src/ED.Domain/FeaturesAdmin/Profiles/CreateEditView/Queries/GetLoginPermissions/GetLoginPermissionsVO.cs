namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetLoginPermissionsVO(
            string LoginElectronicSubjectName,
            string ProfileIdentifier,
            GetLoginPermissionsVOPermissions[] Permissions);

        public record GetLoginPermissionsVOPermissions(
            LoginProfilePermissionType Permission,
            int? TemplateId,
            string? TemplateName);
    }
}
