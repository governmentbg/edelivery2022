using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileBasicInfoVO> GetProfileBasicInfoAsync(
            int profileId,
            CancellationToken ct)
        {
            return await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                on p.Id equals tgp.ProfileId

                where p.Id == profileId

                select new GetProfileBasicInfoVO(
                    p.Identifier,
                    tgp.TargetGroupId,
                    p.IsActivated)
            ).SingleAsync(ct);
        }
    }
}
