using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<FindRecipientLegalEntityVO?> FindRecipientLegalEntityAsync(
            string identifier,
            int templateId,
            CancellationToken ct)
        {
            if (templateId == Template.SystemTemplateId)
            {
                FindRecipientLegalEntityVO? vo = await (
                    from p in this.DbContext.Set<Profile>()

                    join le in this.DbContext.Set<LegalEntity>()
                        on p.ElectronicSubjectId equals le.LegalEntityId

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join tg in this.DbContext.Set<TargetGroup>()
                        on tgp.TargetGroupId equals tg.TargetGroupId

                    where p.IsActivated
                        && !p.IsPassive
                        && !p.IsReadOnly
                        && EF.Functions.Like(p.Identifier, identifier)
                        && tg.TargetGroupId == TargetGroup.LegalEntityTargetGroupId

                    select new FindRecipientLegalEntityVO(p.Id, p.ElectronicSubjectName))
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

                FindRecipientLegalEntityVO? vo = await (
                    from p in this.DbContext.Set<Profile>()

                    join le in this.DbContext.Set<LegalEntity>()
                        on p.ElectronicSubjectId equals le.LegalEntityId

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join tg in this.DbContext.Set<TargetGroup>()
                        on tgp.TargetGroupId equals tg.TargetGroupId

                    where p.IsActivated
                        && !p.IsPassive
                        && !p.IsReadOnly
                        && EF.Functions.Like(p.Identifier, identifier)
                        && tg.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                        && templateProfileIds.Contains(p.Id)

                    select new FindRecipientLegalEntityVO(p.Id, p.ElectronicSubjectName))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
        }
    }
}
