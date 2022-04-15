namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetTargetGroupsVO(
            int TargetGroupId,
            string Name);
    }
}
