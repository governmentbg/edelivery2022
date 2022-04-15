using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetBlobTimestampVO> GetBlobTimestampAsync(
            int messageId,
            int blobId,
            CancellationToken ct);
    }
}
