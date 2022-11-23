using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        // TODO: should we (inner) join with login table
        public async Task<FindRecipientIndividualVO?> FindRecipientIndividualAsync(
            string firstName,
            string lastName,
            string identifier,
            int templateId,
            CancellationToken ct)
        {
            if (templateId == Template.SystemTemplateId)
            {
                FindRecipientIndividualVO? vo = await (
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

                    select new FindRecipientIndividualVO(p.Id, p.ElectronicSubjectName))
                    .FirstOrDefaultAsync(ct);

                return vo;
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

                FindRecipientIndividualVO? vo = await (
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
                        && templateProfileIds.Contains(p.Id)

                    select new FindRecipientIndividualVO(p.Id, p.ElectronicSubjectName))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
        }
    }
}
