using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesRegisterQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<GetLoginsByIdentifiersVO[]> GetLoginsByIdentifiersAsync(
            string[] identifiers,
            CancellationToken ct)
        {
            GetLoginsByIdentifiersVO[] vos = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where identifiers.Contains(p.Identifier)
                    && p.IsActivated
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select new GetLoginsByIdentifiersVO(
                    p.Identifier,
                    l.Id))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
