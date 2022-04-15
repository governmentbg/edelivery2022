using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesSendQueryRepository : IEsbMessagesSendQueryRepository
    {
        public async Task<GetTemporaryOrStorageBlobsVO[]> GetTemporaryOrStorageBlobsAsync(
            int profileId,
            int[] blobIds,
            CancellationToken ct)
        {
            GetTemporaryOrStorageBlobsVO[] vos = await (
                from pb in this.DbContext.Set<ProfileBlobAccessKey>()

                join pk in this.DbContext.Set<ProfileKey>()
                    on pb.ProfileKeyId equals pk.ProfileKeyId

                join b in this.DbContext.Set<Blob>()
                    on pb.BlobId equals b.BlobId

                where pb.ProfileId == profileId
                    && (pb.Type == ProfileBlobAccessKeyType.Temporary || pb.Type == ProfileBlobAccessKeyType.Storage)
                    && this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == pb.BlobId)

                select new GetTemporaryOrStorageBlobsVO(
                    pb.ProfileKeyId,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding,
                    b.BlobId,
                    b.FileName,
                    b.Hash!,
                    b.HashAlgorithm!,
                    pb.EncryptedKey))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
