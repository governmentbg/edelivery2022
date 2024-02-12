using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesListQueryRepository
    {
        public record GetInboxVO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            string SenderProfileName,
            string SenderLoginName,
            string RecipientProfileName,
            string RecipientLoginName,
            string Subject,
            string Url,
            string? Rnu,
            int TemplateId);
    }
}
