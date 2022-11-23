using System;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
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
            ForwardStatus ForwardStatusId,
            string TemplateName,
            string? Rnu);
    }
}
