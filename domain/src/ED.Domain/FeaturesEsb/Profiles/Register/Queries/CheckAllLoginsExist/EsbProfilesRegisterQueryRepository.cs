using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<bool> CheckAllLoginsExistAsync(
            string[] identifiers,
            CancellationToken ct)
        {
            int count = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where identifiers.Contains(p.Identifier)
                    && p.IsActivated
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .CountAsync(ct);

            return count == identifiers.Count();
        }
    }
}
