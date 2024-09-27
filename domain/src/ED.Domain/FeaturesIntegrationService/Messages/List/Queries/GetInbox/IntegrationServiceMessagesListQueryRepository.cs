using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IIntegrationServiceMessagesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesListQueryRepository : IIntegrationServiceMessagesListQueryRepository
    {
        public async Task<TableResultVO<GetInboxVO>> GetInboxAsync(
            int profileId,
            bool showNewOnly,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<MessageRecipient, bool>> predicate =
                   PredicateBuilder.True<MessageRecipient>();

            if (showNewOnly)
            {
                predicate = predicate
                    .And(e => e.DateReceived == null);
            }

            TableResultVO<GetInboxVO> vos = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>().Where(predicate)
                    on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id
                    into lj1
                from rl in lj1.DefaultIfEmpty()

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                where rp.Id == profileId
                    && !this.DbContext.Set<Ticket>().Any(t => t.MessageId == mr.MessageId)

                orderby mr.DateReceived == null ? 1 : 0 descending, m.DateSent descending

                select new GetInboxVO(
                    m.MessageId,
                    m.SubjectExtended!,
                    m.DateCreated,
                    m.DateSent!.Value,
                    mr.DateReceived,
                    new GetInboxVOProfile(
                        sp.Id,
                        sp.ElectronicSubjectId,
                        sp.ElectronicSubjectName),
                    new GetInboxVOProfile(
                        rp.Id,
                        rp.ElectronicSubjectId,
                        rp.ElectronicSubjectName),
                    new GetInboxVOLogin(
                        sl.Id,
                        sl.ElectronicSubjectId,
                        sl.ElectronicSubjectName),
                    rl != null
                        ? new GetInboxVOLogin(
                            rl.Id,
                            rl.ElectronicSubjectId,
                            rl.ElectronicSubjectName)
                        : null
                    ))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
