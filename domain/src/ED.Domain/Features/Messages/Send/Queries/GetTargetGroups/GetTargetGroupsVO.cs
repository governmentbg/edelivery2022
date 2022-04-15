namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetTargetGroupsVO(
            int TargetGroupId,
            string Name);
    }
}
