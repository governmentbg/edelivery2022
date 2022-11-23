namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetForwardMessageInfoVO(
            string Subject,
            string? Rnu);
    }
}
