using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<TableResultVO<GetRecipientGroupsVO>> GetRecipientGroupsAsync(
            string? term,
            int profileId,
            int templateId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<RecipientGroup, bool>> recipientGroupPredicate =
                BuildRecipientGroupPredicate(term);

            if (templateId == Template.SystemTemplateId)
            {
                TableResultVO<GetRecipientGroupsVO> vos = await (
                    from rg in this.DbContext.Set<RecipientGroup>().Where(recipientGroupPredicate)

                    where (rg.ProfileId == profileId || rg.IsPublic)
                        && this.DbContext.Set<RecipientGroupProfile>().Any(r => r.RecipientGroupId == rg.RecipientGroupId) // non-empty groups

                    select new GetRecipientGroupsVO(
                        rg.RecipientGroupId,
                        $"{(rg.IsPublic ? ": " : string.Empty)}{rg.Name}"))
                    .ToTableResultAsync(offset, limit, ct);

                return vos;
            }
            else
            {
                IQueryable<int> templateProfileIds =
                (from tp in this.DbContext.Set<TemplateProfile>()

                 where tp.TemplateId == templateId && tp.CanReceive

                 select tp.ProfileId).Concat(
                    from ttg in this.DbContext.Set<TemplateTargetGroup>()

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on ttg.TargetGroupId equals tgp.TargetGroupId

                    where ttg.TemplateId == templateId && ttg.CanReceive

                    select tgp.ProfileId);

                TableResultVO<GetRecipientGroupsVO> vos = await (
                    from rg in this.DbContext.Set<RecipientGroup>().Where(recipientGroupPredicate)

                    join rgp in this.DbContext.Set<RecipientGroupProfile>()
                        on rg.RecipientGroupId equals rgp.RecipientGroupId

                    where (rg.ProfileId == profileId || rg.IsPublic)
                        && this.DbContext.Set<RecipientGroupProfile>().Any(r => r.RecipientGroupId == rg.RecipientGroupId) // non-empty groups
                        && templateProfileIds.Contains(rgp.ProfileId)

                    group rg by new
                    {
                        rg.RecipientGroupId,
                        rg.IsPublic,
                        rg.Name,
                    } 
                    into g1

                    orderby g1.Key.RecipientGroupId

                    select new GetRecipientGroupsVO(
                        g1.Key.RecipientGroupId,
                        $"{(g1.Key.IsPublic ? ": " : string.Empty)}{g1.Key.Name}"))
                    .ToTableResultAsync(offset, limit, ct);

                return vos;
            }

            Expression<Func<RecipientGroup, bool>> BuildRecipientGroupPredicate(
                string? term)
            {
                Expression<Func<RecipientGroup, bool>> predicate = PredicateBuilder
                    .True<RecipientGroup>()
                    .And(rg => rg.ArchiveDate == null);

                if (!string.IsNullOrEmpty(term))
                {
                    predicate = predicate
                        .And(rg => EF.Functions.Like(rg.Name, $"%{term}%"));
                }

                return predicate;
            }
        }
    }
}
