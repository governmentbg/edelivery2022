using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetBlobTimestampVO> GetBlobTimestampAsync(
            int messageId,
            int blobId,
            CancellationToken ct)
        {
            GetBlobTimestampVO vo = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where m.MessageId == messageId && b.BlobId == blobId

                select new GetBlobTimestampVO(
                    $"{b.FileName}.tsr",
                    b.Timestamp)
                )
                .SingleAsync(ct);

            return vo;
        }
    }
}
