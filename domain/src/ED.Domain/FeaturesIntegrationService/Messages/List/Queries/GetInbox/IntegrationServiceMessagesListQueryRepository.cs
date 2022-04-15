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
            string certificateThumbprint,
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
                from l in this.DbContext.Set<Login>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                join rp in this.DbContext.Set<Profile>()
                    on lp.ProfileId equals rp.Id

                join mr in this.DbContext.Set<MessageRecipient>().Where(predicate)
                    on rp.Id equals mr.ProfileId

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id
                    into lj1
                from rl in lj1.DefaultIfEmpty()

                join m in this.DbContext.Set<Message>()
                    on mr.MessageId equals m.MessageId

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                where l.IsActive
                    && l.CertificateThumbprint == certificateThumbprint
                    && lp.IsDefault

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
