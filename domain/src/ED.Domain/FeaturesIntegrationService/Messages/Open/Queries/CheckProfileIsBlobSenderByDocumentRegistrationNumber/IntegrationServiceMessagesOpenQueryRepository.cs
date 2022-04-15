using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<bool> CheckProfileIsBlobSenderByDocumentRegistrationNumberAsync(
            int profileId,
            string documentRegistrationNumber,
            CancellationToken ct)
        {
            bool checkProfileIsBlobSender = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where EF.Functions.Like(b.DocumentRegistrationNumber, documentRegistrationNumber)
                    && m.SenderProfileId == profileId

                select m.MessageId)
                .AnyAsync(ct);

            return checkProfileIsBlobSender;
        }
    }
}
