using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesListQueryRepository
    {
        public record GetOutboxVO(
            int MessageId,
            DateTime DateSent,
            string SenderProfileName,
            string SenderLoginName,
            string Recipients,
            string Subject,
            string Url,
            string? Orn,
            string? ReferencedOrn,
            string? AdditionalIdentifier);
    }
}
