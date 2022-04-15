using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesSendQueryRepository : IEsbMessagesSendQueryRepository
    {
        public async Task<GetBlobsInfoVO[]> GetBlobsInfoAsync(
            int[] blobIds,
            CancellationToken ct)
        {
            GetBlobsInfoVO[] vos = await (
                from b in this.DbContext.Set<Blob>()

                where this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == b.BlobId)

                select new GetBlobsInfoVO(
                    b.BlobId,
                    b.FileName,
                    b.HashAlgorithm,
                    b.Hash,
                    b.Size))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
