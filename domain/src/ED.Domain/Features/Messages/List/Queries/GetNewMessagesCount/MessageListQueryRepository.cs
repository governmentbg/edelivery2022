using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageListQueryRepository;

namespace ED.Domain
{
    partial class MessageListQueryRepository : IMessageListQueryRepository
    {
        public async Task<GetNewMessagesCountVO[]> GetNewMessagesCountAsync(
            int loginId,
            CancellationToken ct)
        {
            var profileMessages = await (
                from p in this.DbContext.Set<Profile>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on p.Id equals lp.ProfileId

                join mr in this.DbContext.Set<MessageRecipient>()
                    on p.Id equals mr.ProfileId

                where p.IsActivated
                    && lp.LoginId == loginId
                    && !mr.DateReceived.HasValue
                    && !this.DbContext.Set<Ticket>().Any(t => t.MessageId == mr.MessageId)

                select new { ProfileId = p.Id, mr.MessageId })
                .ToArrayAsync(ct);

            GetNewMessagesCountVO[] vos = profileMessages
                .GroupBy(e => e.ProfileId)
                .Select(e => new GetNewMessagesCountVO(e.Key, e.Count()))
                .ToArray();

            return vos;
        }
    }
}
