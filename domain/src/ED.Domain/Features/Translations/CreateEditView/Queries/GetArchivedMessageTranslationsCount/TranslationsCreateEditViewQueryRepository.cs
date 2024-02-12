using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class TranslationsCreateEditViewQueryRepository : ITranslationsCreateEditViewQueryRepository
    {
        public async Task<int> GetArchivedMessageTranslationsCountAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            int count = await (
                from mt in this.DbContext.Set<MessageTranslation>()

                where mt.MessageId == messageId
                    && mt.ProfileId == profileId
                    && mt.ArchiveDate != null

                select mt.MessageId)
                .CountAsync(ct);

            return count;
        }
    }
}
