namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindProfilesVO(
            int ProfileId,
            string Name);
    }
}
