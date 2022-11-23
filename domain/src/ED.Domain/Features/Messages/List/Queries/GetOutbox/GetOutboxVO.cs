using System;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        public record GetOutboxVO(
            int MessageId,
            DateTime DateSent,
            string SenderProfileName,
            string SenderLoginName,
            string Recipients,
            string Subject,
            ForwardStatus ForwardStatusId,
            string TemplateName,
            int NumberOfRecipients,
            int NumberOfTotalRecipients,
            string? Rnu);
    }
}
