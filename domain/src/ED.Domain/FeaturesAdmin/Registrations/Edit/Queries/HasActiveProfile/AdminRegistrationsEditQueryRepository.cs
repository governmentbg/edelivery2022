using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<bool> HasActiveProfileAsync(
            int profileId,
            string identifier,
            int targetGroupId,
            CancellationToken ct)
        {
            bool hasActiveProfile = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where p.Id != profileId
                    && p.Identifier == identifier
                    && tgp.TargetGroupId == (int)targetGroupId
                    && p.IsActivated

                select p)
                .AnyAsync(ct);

            return hasActiveProfile;
        }
    }
}
