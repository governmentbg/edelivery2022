using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IEsbMessagesListQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesListQueryRepository : IEsbMessagesListQueryRepository
    {
        public async Task<TableResultVO<GetInboxVO>> GetInboxAsync(
            int profileId,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            // Use the DateTime value of the SqlDateTime.MaxValue.
            // EF serializes the SqlDateTime to string which ends up as
            // '31-Dec-99 23:59:59', which is not the intended 9999 year.
            DateTime sqlMaxDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

            TableResultVO<GetInboxVO> vos = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

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

                where mr.ProfileId == profileId

                orderby mr.DateReceived ?? sqlMaxDate descending, m.DateSent descending

                select new GetInboxVO(
                    m.MessageId,
                    m.DateSent!.Value,
                    mr.DateReceived,
                    sp.ElectronicSubjectName,
                    sl.ElectronicSubjectName,
                    rp.ElectronicSubjectName,
                    rl != null ? rl.ElectronicSubjectName : string.Empty,
                    m.Subject,
                    $"{this.domainOptions.WebPortalUrl}/Messages/Open/{m.MessageId}",
                    m.Rnu))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
