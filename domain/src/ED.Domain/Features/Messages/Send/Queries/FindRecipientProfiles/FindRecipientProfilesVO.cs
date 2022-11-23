namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindRecipientProfilesVO(
            int ProfileId,
            string Name);
    }
}
