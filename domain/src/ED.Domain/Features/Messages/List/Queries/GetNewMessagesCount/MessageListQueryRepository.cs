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
            GetNewMessagesCountVO[] vos = await (
                from p in this.DbContext.Set<Profile>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on p.Id equals lp.ProfileId

                join mr in this.DbContext.Set<MessageRecipient>()
                    on p.Id equals mr.ProfileId

                where p.IsActivated
                    && lp.LoginId == loginId
                    && !mr.DateReceived.HasValue

                group p by p.Id into g

                select new GetNewMessagesCountVO(
                    g.Key,
                    g.Count()))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
