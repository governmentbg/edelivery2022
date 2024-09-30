using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository
    {
        public async Task<TableResultVO<FindRecipientProfilesVO>> FindRecipientProfilesAsync(
            string term,
            int? targetGroupId,
            int templateId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<Profile, bool>> profilePredicate =
                BuildProfilePredicate(term);

            Expression<Func<TargetGroup, bool>> targetGroupPredicate =
                BuildTargetGroupPredicate(targetGroupId);

            if (templateId == Template.SystemTemplateId)
            {
                TableResultVO<FindRecipientProfilesVO> vos = await (
                    from p in this.DbContext.Set<Profile>().Where(profilePredicate)

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join tg in this.DbContext.Set<TargetGroup>().Where(targetGroupPredicate)
                        on tgp.TargetGroupId equals tg.TargetGroupId

                    orderby p.ElectronicSubjectName

                    select new FindRecipientProfilesVO(
                        p.Id,
                        p.ElectronicSubjectName))
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

                TableResultVO<FindRecipientProfilesVO> vos = await (
                    from p in this.DbContext.Set<Profile>().Where(profilePredicate)

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join tg in this.DbContext.Set<TargetGroup>().Where(targetGroupPredicate)
                        on tgp.TargetGroupId equals tg.TargetGroupId

                    where templateProfileIds.Contains(p.Id)

                    orderby p.ElectronicSubjectName

                    select new FindRecipientProfilesVO(
                        p.Id,
                        p.ElectronicSubjectName))
                    .ToTableResultAsync(offset, limit, ct);

                return vos;
            }

            Expression<Func<Profile, bool>> BuildProfilePredicate(string term)
            {
                Expression<Func<Profile, bool>> predicate = PredicateBuilder
                    .True<Profile>()
                    .And(p => p.IsActivated)
                    .And(p => !p.HideAsRecipient);

                if (!string.IsNullOrEmpty(term))
                {
                    predicate = predicate
                        .And(p =>
                            EF.Functions.Like(p.ElectronicSubjectName, $"%{term}%")
                            || EF.Functions.Like(p.Identifier, $"%{term}%"));
                }

                return predicate;
            }

            Expression<Func<TargetGroup, bool>> BuildTargetGroupPredicate(int? targetGroupId)
            {
                Expression<Func<TargetGroup, bool>> predicate = PredicateBuilder
                    .True<TargetGroup>()
                    .And(tg => tg.ArchiveDate == null);

                if (targetGroupId == null)
                {
                    predicate = predicate
                        .And(tg => tg.TargetGroupId != TargetGroup.IndividualTargetGroupId
                            && tg.TargetGroupId != TargetGroup.LegalEntityTargetGroupId);
                }
                else
                {
                    predicate = predicate
                        .And(tg => tg.TargetGroupId == targetGroupId);
                }

                return predicate;
            }
        }
    }
}
