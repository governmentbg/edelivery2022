using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<GetAllStatisticsVO[]> GetAllStatisticsAsync(CancellationToken ct);
    }
}
