using System;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        public record GetForwardHistoryVO(
            int MessageId,
            string SenderName,
            string RecipientName,
            DateTime DateSent,
            DateTime? DateReceived);
    }
}
