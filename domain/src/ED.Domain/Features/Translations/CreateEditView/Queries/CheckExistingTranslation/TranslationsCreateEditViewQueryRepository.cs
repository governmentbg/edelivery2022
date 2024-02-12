using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class TranslationsCreateEditViewQueryRepository : ITranslationsCreateEditViewQueryRepository
    {
        public async Task<bool> CheckExistingTranslationAsync(
            int messageId,
            int profileId,
            string sourceLanguage,
            string targetLanguage,
            CancellationToken ct)
        {
            bool existingTranslation = await (
                from mt in this.DbContext.Set<MessageTranslation>()

                where mt.ProfileId == profileId
                    && mt.MessageId == messageId
                    && mt.SourceLanguage == sourceLanguage
                    && mt.TargetLanguage == targetLanguage
                    && mt.ArchiveDate == null

                select  mt.MessageId)
            .AnyAsync(ct);

            return existingTranslation;
        }
    }
}
