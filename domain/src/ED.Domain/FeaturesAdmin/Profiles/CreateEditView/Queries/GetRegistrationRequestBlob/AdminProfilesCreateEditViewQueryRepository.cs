using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetBlobVO> GetBlobAsync(
            int blobId,
            CancellationToken ct)
        {
            GetBlobVO vo = await (
                from pbak in this.DbContext.Set<ProfileBlobAccessKey>()
                join pk in this.DbContext.Set<ProfileKey>()
                    on pbak.ProfileKeyId equals pk.ProfileKeyId

                where pbak.BlobId == blobId

                select new GetBlobVO(
                    pbak.BlobId,
                    pbak.CreatedByAdminUserId,
                    pbak.EncryptedKey,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding))
                .SingleAsync(ct);

            return vo;
        }
    }
}
