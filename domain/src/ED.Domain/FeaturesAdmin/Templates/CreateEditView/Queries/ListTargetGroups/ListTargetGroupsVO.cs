namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record ListTargetGroupsVO(
            int TargetGroupId,
            string TargetGroupName);
    }
}
