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
            int profileId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetOutboxVO> vos = await (
                from m in this.DbContext.Set<Message>()

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

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

                where sp.Id == profileId

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
