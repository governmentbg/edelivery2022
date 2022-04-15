using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<int> GetReceivedMessagesCountAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct);
    }
}
