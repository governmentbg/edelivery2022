using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<FindIndividualVO?> FindIndividualAsync(
            string firstName,
            string lastName,
            string identifier,
            CancellationToken ct)
        {
            FindIndividualVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where p.IsActivated
                    && !p.IsPassive
                    && !p.IsReadOnly
                    && EF.Functions.Like(i.FirstName, firstName)
                    && EF.Functions.Like(i.LastName, lastName)
                    && EF.Functions.Like(p.Identifier, identifier)
                    && tg.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select new FindIndividualVO(p.Id, p.ElectronicSubjectName))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
