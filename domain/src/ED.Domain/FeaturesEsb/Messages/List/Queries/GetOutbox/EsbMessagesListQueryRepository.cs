using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IEsbMessagesListQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesListQueryRepository : IEsbMessagesListQueryRepository
    {
        public async Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            int profileId,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            TableResultVO<GetOutboxVO> vos = await (
                from m in this.DbContext.Set<Message>()

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                where m.SenderProfileId == profileId

                orderby m.DateSent descending

                select new GetOutboxVO(
                    m.MessageId,
                    m.DateSent!.Value,
                    sp.ElectronicSubjectName,
                    sl.ElectronicSubjectName,
                    m.RecipientsAsText,
                    m.Subject,
                    $"{this.domainOptions.WebPortalUrl}/Messages/View/{m.MessageId}",
                    m.Orn,
                    m.ReferencedOrn,
                    m.AdditionalIdentifier))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
