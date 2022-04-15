using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminRegistrationsEditQueryRepository;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<GetRegistrationRequestBlobVO> GetRegistrationRequestBlobAsync(
            int registrationRequestId,
            CancellationToken ct)
        {
            GetRegistrationRequestBlobVO vo = await (
                from rr in this.DbContext.Set<RegistrationRequest>()

                join pbak in this.DbContext.Set<ProfileBlobAccessKey>()
                    on rr.BlobId equals pbak.BlobId

                join pk in this.DbContext.Set<ProfileKey>()
                    on pbak.ProfileKeyId equals pk.ProfileKeyId

                where rr.RegistrationRequestId == registrationRequestId

                select new GetRegistrationRequestBlobVO(
                    rr.BlobId,
                    rr.CreatedBy,
                    pbak.EncryptedKey,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding))
                .SingleAsync(ct);

            return vo;
        }
    }
}
