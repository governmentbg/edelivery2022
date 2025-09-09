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
        public async Task<TableResultVO<GetInboxVO>> GetInboxAsync(
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

                join m in this.DbContext.Set<Message>().Where(predicate)
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
