using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<int> GetProfileTargetGroupAsync(
            int profileId,
            CancellationToken ct)
        {
            return await (
                from tgp in this.DbContext.Set<TargetGroupProfile>()
                where tgp.ProfileId == profileId
                select tgp.TargetGroupId
            ).SingleAsync(ct);
        }
    }
}
