namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetProfileMessageRoleVO(
            bool IsSender,
            bool IsRecipient);
    }
}
