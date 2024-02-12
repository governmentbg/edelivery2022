using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITicketsListQueryRepository
    {
        Task<GetNewTicketsCountQO[]> GetNewTicketsCountAsync(
            int loginId,
            CancellationToken ct);
    }
}
