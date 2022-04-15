namespace ED.Domain
{
    public partial interface IEsbTargetGroupsListQueryRepository
    {
        public record GetTargetGroupsVO(
            int TargetGroupId,
            string Name,
            bool CanSelectRecipients);
    }
}
