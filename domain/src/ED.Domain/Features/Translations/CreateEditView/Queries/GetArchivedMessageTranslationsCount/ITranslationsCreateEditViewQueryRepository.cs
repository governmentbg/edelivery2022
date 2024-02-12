using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsCreateEditViewQueryRepository
    {
        Task<int> GetArchivedMessageTranslationsCountAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
