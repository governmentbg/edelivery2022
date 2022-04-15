using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IIntegrationServiceMessagesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesListQueryRepository : IIntegrationServiceMessagesListQueryRepository
    {
        public async Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            string certificateThumbprint,
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetOutboxVO> vos = await (
                from l in this.DbContext.Set<Login>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                join sp in this.DbContext.Set<Profile>()
                    on lp.ProfileId equals sp.Id

                join m in this.DbContext.Set<Message>()
                    on sp.Id equals m.SenderProfileId

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id
                    into lj1
                from rl in lj1.DefaultIfEmpty()

                where l.IsActive
                    && l.CertificateThumbprint == certificateThumbprint
                    && sp.IsActivated
                    && lp.IsDefault

                orderby m.DateSent descending

                select new GetOutboxVO(
                    m.MessageId,
                    m.SubjectExtended!,
                    m.DateCreated,
                    m.DateSent!.Value,
                    mr.DateReceived,
                    new GetOutboxVOProfile(
                        sp.Id,
                        sp.ElectronicSubjectId,
                        sp.ElectronicSubjectName),
                    new GetOutboxVOProfile(
                        rp.Id,
                        rp.ElectronicSubjectId,
                        rp.ElectronicSubjectName),
                    new GetOutboxVOLogin(
                        sl.Id,
                        sl.ElectronicSubjectId,
                        sl.ElectronicSubjectName),
                    rl != null 
                        ? new GetOutboxVOLogin(
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
