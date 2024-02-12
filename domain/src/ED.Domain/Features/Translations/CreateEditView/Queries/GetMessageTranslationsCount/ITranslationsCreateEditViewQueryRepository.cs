using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsCreateEditViewQueryRepository
    {
        Task<int> GetMessageTranslationsCountAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
