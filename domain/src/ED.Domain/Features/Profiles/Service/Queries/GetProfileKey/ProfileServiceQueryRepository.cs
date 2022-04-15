using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileServiceQueryRepository;

namespace ED.Domain
{
    partial class ProfileServiceQueryRepository : IProfileServiceQueryRepository
    {
        public async Task<GetProfileKeyVO> GetProfileKeyAsync(
            int profileKeyId,
            CancellationToken ct)
        {
            GetProfileKeyVO vos = await (
                from pk in this.DbContext.Set<ProfileKey>()

                where pk.ProfileKeyId == profileKeyId

                select new GetProfileKeyVO(
                    pk.ProfileKeyId,
                    pk.ProfileId,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding))
                .SingleAsync(ct);

            return vos;
        }
    }
}
