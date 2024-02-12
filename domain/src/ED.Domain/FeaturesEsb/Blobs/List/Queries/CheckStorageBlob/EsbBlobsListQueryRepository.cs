using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbBlobsListQueryRepository;

namespace ED.Domain
{
    partial class EsbBlobsListQueryRepository : IEsbBlobsListQueryRepository
    {
        public async Task<CheckStorageBlobVO> CheckStorageBlobAsync(
            int profileId,
            int blobId,
            CancellationToken ct)
        {
            bool isThere = await (
                from b in this.DbContext.Set<Blob>()

                join pbak in this.DbContext.Set<ProfileBlobAccessKey>()
                    on b.BlobId equals pbak.BlobId

                where pbak.ProfileId == profileId
                    && b.BlobId == blobId
                    && pbak.Type == ProfileBlobAccessKeyType.Storage

                select b.BlobId)
                .AnyAsync(ct);

            return new CheckStorageBlobVO(isThere);
        }
    }
}
