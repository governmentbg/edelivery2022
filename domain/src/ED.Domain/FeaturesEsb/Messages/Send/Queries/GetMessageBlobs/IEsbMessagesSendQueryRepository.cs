using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        Task<GetMessageBlobsVO[]> GetMessageBlobsAsync(
            int profileId,
            int messageId,
            int[] blobIds,
            CancellationToken ct);
    }
}
