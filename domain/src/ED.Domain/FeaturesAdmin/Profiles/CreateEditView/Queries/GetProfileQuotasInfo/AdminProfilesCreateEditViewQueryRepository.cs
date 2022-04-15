using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileQuotasInfoVO> GetProfileQuotasInfoAsync(
            int profileId,
            CancellationToken ct)
        {
            GetProfileQuotasInfoVO? vo = await (
                from pq in this.DbContext.Set<ProfileQuota>()

                where pq.ProfileId == profileId

                select new GetProfileQuotasInfoVO(pq.StorageQuotaInMb))
                .SingleOrDefaultAsync(ct);

            if (vo == null)
            {
                vo = new GetProfileQuotasInfoVO(null);
            }

            return vo;
        }
    }
}
