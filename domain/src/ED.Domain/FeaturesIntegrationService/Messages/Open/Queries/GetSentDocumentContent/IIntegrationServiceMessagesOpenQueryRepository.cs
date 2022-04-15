using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetSentDocumentContentVO?> GetSentDocumentContentAsync(
            int profileId,
            int blobId,
            CancellationToken ct);
    }
}
