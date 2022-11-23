namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetMessageReplyInfoVO(
            int RecipientProfileId,
            string RecipientName,
            int? ResponseTemplateId,
            string Subject,
            string? Rnu);
    }
}
