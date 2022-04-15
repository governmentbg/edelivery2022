namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetSenderProfileVO(
            int ProfileId,
            string Email,
            string Name,
            string Phone,
            string Identifier,
            ProfileType Type);
    }
}
