using System;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetNotificationsVO(
            QueueMessageType Type,
            int Sent,
            int Error,
            DateTime Date);
    }
}
