using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTemplatesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesCreateEditViewQueryRepository : IAdminTemplatesCreateEditViewQueryRepository
    {
        public async Task<ListTargetGroupsVO[]> ListTargetGroupsAsync(
            string term,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<TargetGroup, bool>> predicate = PredicateBuilder
                .True<TargetGroup>()
                .And(e => e.ArchiveDate == null);

            if (!string.IsNullOrEmpty(term))
            {
                predicate = predicate
                    .And(tg => EF.Functions.Like(tg.Name, $"%{term}%"));
            }

            return await
                (from tg in this.DbContext.Set<TargetGroup>().Where(predicate)
                orderby tg.TargetGroupId
                select new ListTargetGroupsVO(
                    tg.TargetGroupId,
                    tg.Name))
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync(ct);
        }
    }
}
