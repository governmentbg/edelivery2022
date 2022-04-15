using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminRegistrationsEditQueryRepository;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<GetRegistrationSystemProfileBlobVO> GetRegistrationSystemProfileBlobAsync(
            int blobId,
            CancellationToken ct)
        {
            GetRegistrationSystemProfileBlobVO vo = await (
                from pbak in this.DbContext.Set<ProfileBlobAccessKey>()

                join pk in this.DbContext.Set<ProfileKey>()
                    on pbak.ProfileKeyId equals pk.ProfileKeyId

                where pbak.BlobId == blobId
                    && pbak.ProfileId == Profile.SystemProfileId

                select new GetRegistrationSystemProfileBlobVO(
                    pbak.BlobId,
                    pbak.EncryptedKey,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding))
                .SingleAsync(ct);

            return vo;
        }
    }
}
