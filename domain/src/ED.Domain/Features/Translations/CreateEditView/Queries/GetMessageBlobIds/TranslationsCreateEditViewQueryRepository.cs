using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class TranslationsCreateEditViewQueryRepository : ITranslationsCreateEditViewQueryRepository
    {
        public async Task<int[]> GetMessageBlobIdsAsync(
            int messageId,
            CancellationToken ct)
        {
            int[] blobIds = await (
                from mb in this.DbContext.Set<MessageBlob>()

                where mb.MessageId == messageId

                select mb.BlobId)
                .ToArrayAsync(ct);

            return blobIds;
        }
    }
}
