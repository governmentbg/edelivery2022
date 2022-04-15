namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetLoginPermissionsVO(
            string LoginName,
            string ProfileIdentifier,
            GetLoginPermissionsVOPermissions[] Permissions);

        public record GetLoginPermissionsVOPermissions(
            LoginProfilePermissionType Permission,
            int? TemplateId,
            string? TemplateName);
    }
}
