namespace ED.Domain
{
    public partial interface IAdminTargetGroupsCreateEditViewQueryRepository
    {
        public record GetTargetGroupMatrixVO(
            int TargetGroupId,
            string Name);
    }
}
