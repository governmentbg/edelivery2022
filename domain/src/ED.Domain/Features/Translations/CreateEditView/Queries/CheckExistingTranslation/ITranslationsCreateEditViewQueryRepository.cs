using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITranslationsCreateEditViewQueryRepository
    {
        Task<bool> CheckExistingTranslationAsync(
            int messageId,
            int profileId,
            string sourceLanguage,
            string targetLanguage,
            CancellationToken ct);
    }
}
