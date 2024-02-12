using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<bool> IsRecipientProfileIndividualAsync(
            int messageId,
            CancellationToken ct)
        {
            bool isIndividual = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where mr.MessageId == messageId
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .Cast<int?>()
                .AnyAsync(ct);

            return isIndividual;
        }
    }
}
