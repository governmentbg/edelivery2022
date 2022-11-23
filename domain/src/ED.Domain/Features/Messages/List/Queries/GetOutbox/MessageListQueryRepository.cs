using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageListQueryRepository;

namespace ED.Domain
{
    partial class MessageListQueryRepository
    {
        public async Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            int profileId,
            int offset,
            int limit,
            string? subject,
            string? profile,
            DateTime? fromDate,
            DateTime? toDate,
            string? rnu,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    profileId,
                    subject,
                    profile,
                    fromDate,
                    toDate,
                    rnu);

            int count = await this.DbContext.Set<Message>()
                .Where(messagePredicate)
                .CountAsync(ct);

            if (count == 0)
            {
                return TableResultVO.Empty<GetOutboxVO>();
            }

            GetOutboxVO[] vos = await (
                from m in this.DbContext.Set<Message>().Where(messagePredicate)

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join l1 in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l1.Id

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                group mr by new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    ProfileName = p1.ElectronicSubjectName,
                    LoginName = l1.ElectronicSubjectName,
                    m.RecipientsAsText,
                    m.SubjectExtended,
                    m.ForwardStatusId,
                    t.Name,
                    m.Rnu,
                } into g

                orderby g.Key.DateSent descending

                select new GetOutboxVO(
                    g.Key.MessageId,
                    g.Key.DateSent,
                    g.Key.ProfileName,
                    g.Key.LoginName,
                    g.Key.RecipientsAsText,
                    g.Key.SubjectExtended!,
                    g.Key.ForwardStatusId,
                    g.Key.Name,
                    g.Count(gr => gr.DateReceived.HasValue),
                    g.Count(),
                    g.Key.Rnu))
                .WithOffsetAndLimit(offset, limit)
                .ToArrayAsync(ct);

            return new TableResultVO<GetOutboxVO>(vos, count);

            static Expression<Func<Message, bool>> BuildMessagePredicate(
                int profileId,
                string? subject,
                string? profile,
                DateTime? fromDate,
                DateTime? toDate,
                string? rnu)
            {
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>();

                predicate = predicate.And(e => e.SenderProfileId == profileId);

                if (!string.IsNullOrEmpty(subject))
                {
                    predicate = predicate
                        .And(e => e.Subject.Contains(subject));
                }

                if (!string.IsNullOrEmpty(profile))
                {
                    predicate = predicate
                        .And(e => e.RecipientsAsText.Contains(profile));
                }

                if (fromDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value > fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value < toDate.Value.AddDays(1));
                }

                if (!string.IsNullOrEmpty(rnu))
                {
                    predicate = predicate
                        .And(e => e.Rnu == rnu);
                }

                return predicate;
            }
        }
    }
}
