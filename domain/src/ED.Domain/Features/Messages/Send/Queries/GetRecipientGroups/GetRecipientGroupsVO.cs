namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetRecipientGroupsVO(
            int RecipientGroupId,
            string Name);
    }
}
