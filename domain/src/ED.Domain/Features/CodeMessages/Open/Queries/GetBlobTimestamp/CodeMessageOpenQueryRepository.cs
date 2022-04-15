using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<GetBlobTimestampVO> GetBlobTimestampAsync(
            string accessCode,
            int blobId,
            CancellationToken ct)
        {
            GetBlobTimestampVO vo = await (
                from mac in this.DbContext.Set<MessagesAccessCode>()

                join m in this.DbContext.Set<Message>()
                    on mac.MessageId equals m.MessageId

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where EF.Functions.Like(mac.AccessCode.ToString(), accessCode)
                    && b.BlobId == blobId

                select new GetBlobTimestampVO(
                    $"{b.FileName}.tsr",
                    b.Timestamp)
                )
                .SingleAsync(ct);

            return vo;
        }
    }
}
