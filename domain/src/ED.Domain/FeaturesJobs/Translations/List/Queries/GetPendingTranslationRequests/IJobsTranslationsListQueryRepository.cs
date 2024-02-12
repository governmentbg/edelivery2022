using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IJobsTranslationsListQueryRepository
    {
        Task<GetPendingTranslationRequestsVO[]> GetPendingTranslationRequestsAsync(
            int messageTranslationId,
            CancellationToken ct);
    }
}
