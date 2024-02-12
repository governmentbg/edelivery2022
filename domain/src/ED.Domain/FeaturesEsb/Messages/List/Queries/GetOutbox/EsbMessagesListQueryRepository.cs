using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IEsbMessagesListQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesListQueryRepository : IEsbMessagesListQueryRepository
    {
        public async Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            int profileId,
            DateTime? from,
            DateTime? to,
            int? templateId,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> predicate =
                BuildMessagePredicate(from, to, templateId);

            TableResultVO<GetOutboxVO> vos = await (
                from m in this.DbContext.Set<Message>().Where(predicate)

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
                    m.Rnu,
                    m.TemplateId!.Value))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<Message, bool>> BuildMessagePredicate(
                DateTime? from,
                DateTime? to,
                int? templateId)
            {
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>();

                if (from.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent >= from.Value);
                }

                if (to.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent <= to.Value);
                }

                if (templateId.HasValue)
                {
                    predicate = predicate
                        .And(e => e.TemplateId == templateId.Value);
                }

                return predicate;
            }
        }
    }
}
