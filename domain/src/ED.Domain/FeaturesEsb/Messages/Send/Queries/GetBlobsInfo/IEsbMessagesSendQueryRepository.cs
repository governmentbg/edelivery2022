using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        Task<GetBlobsInfoVO[]> GetBlobsInfoAsync(
            int[] blobIds,
            CancellationToken ct);
    }
}
