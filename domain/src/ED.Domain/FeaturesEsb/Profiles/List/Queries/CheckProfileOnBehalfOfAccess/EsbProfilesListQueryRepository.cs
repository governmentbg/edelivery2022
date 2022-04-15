using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<bool> CheckProfileOnBehalfOfAccessAsync(
            int profileId,
            CancellationToken ct)
        {
            bool hasAccess = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>() // TODO: get integration login
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join lp in this.DbContext.Set<LoginProfile>()
                    on new { ProfileId = p.Id, LoginId = l.Id } equals new { lp.ProfileId, lp.LoginId }

                where p.Id == profileId
                    && p.IsActivated
                    && l.CanSendOnBehalfOf.HasValue
                    && l.CanSendOnBehalfOf.Value
                    && lp.IsDefault

                select p.Id)
                .AnyAsync(ct);

            return hasAccess;
        }
    }
}
