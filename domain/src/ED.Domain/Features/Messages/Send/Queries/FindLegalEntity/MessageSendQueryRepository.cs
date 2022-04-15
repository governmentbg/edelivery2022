using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<FindLegalEntityVO?> FindLegalEntityAsync(
            string identifier,
            CancellationToken ct)
        {
            FindLegalEntityVO? vo = await (
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

                select new FindLegalEntityVO(p.Id, p.ElectronicSubjectName))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
