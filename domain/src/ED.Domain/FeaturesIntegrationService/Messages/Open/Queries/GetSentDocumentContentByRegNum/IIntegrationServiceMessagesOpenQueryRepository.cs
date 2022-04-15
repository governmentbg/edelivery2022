using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetSentDocumentContentByRegNumVO?> GetSentDocumentContentByRegNumAsync(
            int profileId,
            string documentRegistrationNumber,
            CancellationToken ct);
    }
}
