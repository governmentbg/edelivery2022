using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        public async Task<TableResultVO<GetSentMessagesVO>> GetSentMessagesAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            int? recipientProfileId,
            int? senderProfileId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{fromDate}\", \"{toDate}\", \"{recipientProfileId}\", \"{senderProfileId}\", {offset}, {limit}) called",
                nameof(GetSentMessagesAsync),
                adminUserId,
                fromDate,
                toDate,
                recipientProfileId,
                senderProfileId,
                offset,
                limit);

            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    senderProfileId,
                    fromDate,
                    toDate);

            Expression<Func<MessageRecipient, bool>> messageRecipientPredicate =
                BuildMessageRecipientPredicate(recipientProfileId);

            TableResultVO<GetSentMessagesVO> vos = await (
                from m in this.DbContext.Set<Message>().Where(messagePredicate)

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join mr in this.DbContext.Set<MessageRecipient>().Where(messageRecipientPredicate)
                    on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join stgp in this.DbContext.Set<TargetGroupProfile>()
                    on sp.Id equals stgp.ProfileId

                join rtgp in this.DbContext.Set<TargetGroupProfile>()
                    on rp.Id equals rtgp.ProfileId

                join stg in this.DbContext.Set<TargetGroup>()
                    on stgp.TargetGroupId equals stg.TargetGroupId

                join rtg in this.DbContext.Set<TargetGroup>()
                    on rtgp.TargetGroupId equals rtg.TargetGroupId

                orderby m.DateSent descending, m.MessageId descending

                select new GetSentMessagesVO(
                    sp.Id,
                    sp.ElectronicSubjectName,
                    sp.IsActivated,
                    stg.Name,
                    rp.Id,
                    rp.ElectronicSubjectName,
                    rp.IsActivated,
                    rtg.Name,
                    m.MessageId,
                    m.SubjectExtended!,
                    m.DateSent!.Value,
                    mr.DateReceived))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<Message, bool>> BuildMessagePredicate(
                int? senderProfileId,
                DateTime fromDate,
                DateTime toDate)
            {
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>()
                        .And(e => e.DateSent >= fromDate)
                        .And(e => e.DateSent < toDate.AddDays(1));

                if (senderProfileId.HasValue)
                {
                    predicate = predicate
                        .And(e => e.SenderProfileId == senderProfileId.Value);
                }

                return predicate;
            }

            Expression<Func<MessageRecipient, bool>> BuildMessageRecipientPredicate(
               int? recipientProfileId)
            {
                Expression<Func<MessageRecipient, bool>> predicate =
                    PredicateBuilder.True<MessageRecipient>();

                if (recipientProfileId.HasValue)
                {
                    predicate = predicate
                        .And(e => e.ProfileId == recipientProfileId.Value);
                }

                return predicate;
            }
        }
    }
}
