using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesRegisterQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<GetExistingIndividualVO?> GetExistingIndividualAsync(
            string identifier,
            CancellationToken ct)
        {
            GetExistingIndividualVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId
                    into lj1
                from l in lj1.DefaultIfEmpty()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select new GetExistingIndividualVO(
                    p.Id,
                    p.ProfileType,
                    p.IsPassive,
                    l != null))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
