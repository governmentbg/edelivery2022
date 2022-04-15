using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<GetLastStatisticsVO?> GetLastStatisticsAsync(CancellationToken ct);
    }
}
