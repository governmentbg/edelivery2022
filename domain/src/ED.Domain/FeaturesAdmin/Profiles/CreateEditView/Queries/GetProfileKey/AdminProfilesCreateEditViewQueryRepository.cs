using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileKeyVO> GetProfileKeyAsync(
            int profileKeyId,
            CancellationToken ct)
        {
            GetProfileKeyVO vo = await (
                from pk in this.DbContext.Set<ProfileKey>()

                where pk.ProfileKeyId == profileKeyId

                select new GetProfileKeyVO(
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding))
                .SingleAsync(ct);

            return vo;
        }
    }
}
