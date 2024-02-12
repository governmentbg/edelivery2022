using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsListQueryRepository
    {
        Task<GetMessageTranslationVO> GetMessageTranslationAsync(
            int messageTranslationId,
            CancellationToken ct);
    }
}
