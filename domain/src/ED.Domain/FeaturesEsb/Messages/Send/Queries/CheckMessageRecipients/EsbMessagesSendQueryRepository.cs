using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbMessagesSendQueryRepository : IEsbMessagesSendQueryRepository
    {
        public async Task<bool> CheckMessageRecipientsAsync(
            int[] recipientProfileIds,
            int profileId,
            CancellationToken ct)
        {
            var profiles = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tgm in this.DbContext.Set<TargetGroupMatrix>()
                    on tgp.TargetGroupId equals tgm.SenderTargetGroupId

                join rtgp in this.DbContext.Set<TargetGroupProfile>()
                    on tgm.RecipientTargetGroupId equals rtgp.TargetGroupId

                where p.Id == profileId
                    && recipientProfileIds.Contains(rtgp.ProfileId)

                select rtgp.ProfileId)
                .Distinct()
                .ToArrayAsync(ct);

            bool result = recipientProfileIds.All(p => profiles.Contains(p));

            return result;
        }
    }
}
