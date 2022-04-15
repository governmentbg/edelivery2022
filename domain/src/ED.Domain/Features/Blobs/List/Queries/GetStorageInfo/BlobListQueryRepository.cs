using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IBlobListQueryRepository;

namespace ED.Domain
{
    partial class BlobListQueryRepository : IBlobListQueryRepository
    {
        public async Task<GetStorageInfoVO> GetStorageInfoAsync(
            int profileId,
            CancellationToken ct)
        {
            int? quota = await (
                from pq in this.DbContext.Set<ProfileQuota>()

                where pq.ProfileId == profileId

                select pq.StorageQuotaInMb)
                .FirstOrDefaultAsync(ct);

            long usedSpace = await (
                from pss in this.DbContext.Set<ProfileStorageSpace>()

                where pss.ProfileId == profileId

                select pss.UsedStorageSpace)
                .FirstOrDefaultAsync(ct);

            return new GetStorageInfoVO(
                quota.HasValue
                    ? Convert.ToUInt64(quota.Value) * 1024 * 1024
                    : ProfileQuota.DefaultStorageQuotaInBytes,
                Convert.ToUInt64(usedSpace));
        }
    }
}
