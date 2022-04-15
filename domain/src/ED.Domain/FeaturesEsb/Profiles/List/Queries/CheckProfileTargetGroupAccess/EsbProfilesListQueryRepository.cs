using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<bool> CheckProfileTargetGroupAccessAsync(
            int profileId,
            int targetGroupId,
            CancellationToken ct)
        {
            bool hasAccess = await (
               from p in this.DbContext.Set<Profile>()

               join tgp in this.DbContext.Set<TargetGroupProfile>()
                   on p.Id equals tgp.ProfileId

               join tgm in this.DbContext.Set<TargetGroupMatrix>()
                   on tgp.TargetGroupId equals tgm.SenderTargetGroupId

               where p.Id == profileId
                    && tgm.RecipientTargetGroupId == targetGroupId

               select tgm.RecipientTargetGroupId)
               .AnyAsync(ct);

            return hasAccess;
        }
    }
}
