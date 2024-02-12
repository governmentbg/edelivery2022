using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsListQueryRepository
    {
        Task<GetMessageTranslationsVO[]> GetMessageTranslationsAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
