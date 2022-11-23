namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetTargetGroupsFromMatrixVO(
            int TargetGroupId,
            string Name);
    }
}
