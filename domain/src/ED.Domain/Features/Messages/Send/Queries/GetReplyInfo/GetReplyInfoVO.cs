namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetReplyInfoVO(
            int RecipientProfileId,
            string RecipientName,
            int? ResponseTemplateId,
            string Subject,
            string? Orn);
    }
}
