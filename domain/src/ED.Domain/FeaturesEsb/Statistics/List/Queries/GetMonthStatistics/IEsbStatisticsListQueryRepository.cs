using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<GetMonthStatisticsVO?> GetMonthStatisticsAsync(
            DateTime month,
            CancellationToken ct);
    }
}
