using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<bool> CheckProfileIsMessageSenderByDocumentRegistrationNumberAsync(
            int profileId,
            string documentRegistrationNumber,
            CancellationToken ct)
        {
            bool checkProfileIsMessageSender = await (
                from b in this.DbContext.Set<Blob>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on b.BlobId equals mb.BlobId

                join m in this.DbContext.Set<Message>()
                    on mb.MessageId equals m.MessageId

                where EF.Functions.Like(b.DocumentRegistrationNumber, documentRegistrationNumber)
                    && m.SenderProfileId == profileId

                select b.BlobId)
                .AnyAsync(ct);

            return checkProfileIsMessageSender;
        }
    }
}
