namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record GetPermissionsVO(
            GetPermissionsVOProfile[] TemplateProfiles,
            GetPermissionsVOTargetGroup[] TemplateTargetGroups);

        public record GetPermissionsVOProfile(
            int TemplateId,
            int ProfileId,
            string ProfileName,
            bool CanSend,
            bool CanReceive);

        public record GetPermissionsVOTargetGroup(
            int TemplateId,
            int TargetGroupId,
            string TargetGroupName,
            bool CanSend,
            bool CanReceive);
    }
}
