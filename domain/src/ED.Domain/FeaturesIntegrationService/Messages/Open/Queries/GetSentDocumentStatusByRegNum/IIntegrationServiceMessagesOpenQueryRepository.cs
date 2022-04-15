using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetSentDocumentStatusByRegNumVO> GetSentDocumentStatusByRegNumAsync(
            int profileId,
            string? documentRegistrationNumber,
            CancellationToken ct);
    }
}
