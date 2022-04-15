using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        Task<GetTemporaryOrStorageBlobsVO[]> GetTemporaryOrStorageBlobsAsync(
            int profileId,
            int[] blobIds,
            CancellationToken ct);
    }
}
