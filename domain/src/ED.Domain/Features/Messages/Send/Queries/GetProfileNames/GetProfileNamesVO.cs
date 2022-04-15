namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}
