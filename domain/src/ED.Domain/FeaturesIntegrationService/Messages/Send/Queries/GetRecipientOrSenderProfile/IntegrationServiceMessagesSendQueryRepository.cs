using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<GetRecipientOrSenderProfileVO?> GetRecipientOrSenderProfileAsync(
            string identifier,
            int[] targetGroupIds,
            CancellationToken ct)
        {
            // TODO: when looking up individuals should take into account passive profiles as well
            // TODO: when looking for legal entities should take only active profiles
            // TODO: both results should be singleordefault when there is constraint for identifiers in db
            GetRecipientOrSenderProfileVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && targetGroupIds.Contains(tgp.TargetGroupId)

                orderby p.IsActivated descending

                select new GetRecipientOrSenderProfileVO(p.Id, p.ElectronicSubjectName))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
