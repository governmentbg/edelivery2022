using System;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        public record GetMonthStatisticsVO(
            DateTime MonthDate,
            string Month,
            int MessagesSent,
            int MessagesReceived);
    }
}
