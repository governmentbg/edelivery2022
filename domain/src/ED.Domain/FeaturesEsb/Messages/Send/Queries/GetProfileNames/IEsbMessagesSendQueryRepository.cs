using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        Task<GetProfileNamesVO[]> GetProfileNamesAsync(
            int[] profileIds,
            CancellationToken ct);
    }
}
