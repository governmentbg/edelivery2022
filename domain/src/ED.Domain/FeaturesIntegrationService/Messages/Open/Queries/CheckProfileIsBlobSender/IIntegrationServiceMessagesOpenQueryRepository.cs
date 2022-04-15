using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<bool> CheckProfileIsBlobSenderAsync(
            int profileId,
            int blobId,
            CancellationToken ct);
    }
}
