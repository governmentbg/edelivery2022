using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesListQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<SearchGetTargetGroupProfilesVO?> SearchGetTargetGroupProfilesAsync(
            string identifier,
            int? templateId,
            int targetGroupId,
            CancellationToken ct)
        {
            Expression<Func<Profile, bool>> predicate =
                BuildProfilePredicate(identifier, templateId);

            SearchGetTargetGroupProfilesVO? vo = await (
                from p in this.DbContext.Set<Profile>().Where(predicate)

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where (p.IsActivated || (targetGroupId == TargetGroup.IndividualTargetGroupId && p.IsPassive))
                    && tg.TargetGroupId == targetGroupId

                orderby p.Id

                select new SearchGetTargetGroupProfilesVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone))
                .SingleOrDefaultAsync(ct);

            return vo;

            Expression<Func<Profile, bool>> BuildProfilePredicate(
                string identifier,
                int? templateId)
            {
                Expression<Func<Profile, bool>> predicate = PredicateBuilder
                    .True<Profile>()
                    .And(e => EF.Functions.Like(e.Identifier, identifier));

                if (templateId.HasValue)
                {
                    IQueryable<int> allowedProfilesIds = (
                        from p in this.DbContext.Set<Profile>()
                        join tgp in this.DbContext.Set<TargetGroupProfile>()
                            on p.Id equals tgp.ProfileId
                        join ttg in this.DbContext.Set<TemplateTargetGroup>()
                            on tgp.TargetGroupId equals ttg.TargetGroupId
                        where ttg.TemplateId == templateId.Value
                        select p.Id
                        ).Union(
                            from tp in this.DbContext.Set<TemplateProfile>()
                            where tp.TemplateId == templateId.Value
                            select tp.ProfileId
                        );

                    predicate = predicate
                        .And(e => allowedProfilesIds.Contains(e.Id));
                }

                return predicate;
            }
        }
    }
}
