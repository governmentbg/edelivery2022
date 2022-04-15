using System;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        public record GetAllStatisticsVO(
            DateTime MonthDate,
            string Month,
            int MessagesSent,
            int MessagesReceived);
    }
}
