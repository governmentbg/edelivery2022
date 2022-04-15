using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileListQueryRepository;

namespace ED.Domain
{
    partial class ProfileListQueryRepository : IProfileListQueryRepository
    {
        public async Task<TableResultVO<GetTargetGroupProfilesVO>> GetTargetGroupProfilesAsync(
            int targetGroupId,
            string term,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<Profile, bool>> predicate =
                PredicateBuilder.True<Profile>();

            if (!string.IsNullOrEmpty(term))
            {
                predicate = predicate
                    .And(p =>
                        EF.Functions.Like(p.ElectronicSubjectName, $"%{term}%")
                        || EF.Functions.Like(p.Identifier, $"%{term}%"));
            }

            var vos = await (
                from p in this.DbContext.Set<Profile>().Where(predicate)

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where p.IsActivated
                    && !p.IsPassive
                    && tg.TargetGroupId == targetGroupId

                orderby p.ElectronicSubjectName

                select new GetTargetGroupProfilesVO(
                    p.ElectronicSubjectName,
                    p.Identifier))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
