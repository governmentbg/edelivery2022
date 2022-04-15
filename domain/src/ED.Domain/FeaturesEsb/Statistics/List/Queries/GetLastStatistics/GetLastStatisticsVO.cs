using System;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        public record GetLastStatisticsVO(
            DateTime MonthDate,
            string Month,
            int MessagesSent,
            int MessagesReceived);
    }
}
