namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetMessageReplyInfoVO(
            int RecipientProfileId,
            string RecipientName,
            int? ResponseTemplateId,
            string Subject,
            string? Rnu);
    }
}
