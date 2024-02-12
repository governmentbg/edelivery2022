using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IJobsDataPortalListQueryRepository
    {
        Task<GetProfilesMonthlyStatisticsVO[]> GetProfilesMonthlyStatisticsAsync(
            DateTime @from,
            DateTime to,
            CancellationToken ct);
    }
}
