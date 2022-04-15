using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<int> GetSentMessagesCountAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct);
    }
}
