using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsCreateEditViewQueryRepository
    {
        Task<int[]> GetMessageBlobIdsAsync(
            int messageId,
            CancellationToken ct);
    }
}
