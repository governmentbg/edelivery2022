using System.Collections.Generic;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetStatisticsVO(
            int TotalUsers,
#pragma warning disable CA2227 // Collection properties should be read only
            Dictionary<string, int> TargetGroupsCount,
#pragma warning restore CA2227 // Collection properties should be read only
            int TotalMessages,
            int TotalMessagesLast30Days,
            int TotalMessagesLast10Days,
            int TotalMessagesToday);
    }
}
