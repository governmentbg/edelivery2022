using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<bool> CheckProfileIsBlobSenderAsync(
            int profileId,
            int blobId,
            CancellationToken ct)
        {
            bool checkProfileIsBlobSender = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                where mb.BlobId == blobId
                    && m.SenderProfileId == profileId

                select m.MessageId)
                .AnyAsync(ct);

            return checkProfileIsBlobSender;
        }
    }
}
