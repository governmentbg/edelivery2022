using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceStatisticsListQueryRepository
    {
        Task<GetStatisticsVO> GetStatisticsAsync(CancellationToken ct);
    }
}
