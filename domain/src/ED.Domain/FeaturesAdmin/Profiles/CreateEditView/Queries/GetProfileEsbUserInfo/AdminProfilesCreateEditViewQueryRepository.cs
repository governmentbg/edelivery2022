using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileEsbUserInfoVO> GetProfileEsbUserInfoAsync(
            int profileId,
            CancellationToken ct)
        {
            GetProfileEsbUserInfoVO? vo = await (
                from peu in this.DbContext.Set<ProfileEsbUser>()

                where peu.ProfileId == profileId

                select new GetProfileEsbUserInfoVO(
                    peu.OId,
                    peu.ClientId))
                .SingleOrDefaultAsync(ct);

            if (vo == null)
            {
                vo = new GetProfileEsbUserInfoVO(null, null);
            }

            return vo;
        }
    }
}
