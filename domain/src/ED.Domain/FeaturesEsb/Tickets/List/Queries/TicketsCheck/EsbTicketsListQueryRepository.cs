using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbTicketsListQueryRepository;

namespace ED.Domain
{
    partial class EsbTicketsListQueryRepository : IEsbTicketsListQueryRepository
    {
        public async Task<TicketsCheckVO[]> TicketsCheckAsync(
            int[] ticketIds,
            int? deliveryStatus,
            DateTime? from,
            DateTime? to,
            int profileId,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    ticketIds,
                    from,
                    to);

            Expression<Func<MessageRecipient, bool>> messageRecipientPredicate =
                BuildMessageRecipientPredicate(deliveryStatus);

            TicketsCheckVO[] vos = await (
                from m in this.DbContext.Set<Message>().Where(messagePredicate)

                join t in this.DbContext.Set<Ticket>()
                    on m.MessageId equals t.MessageId

                join mr in this.DbContext.Set<MessageRecipient>().Where(messageRecipientPredicate)
                    on m.MessageId equals mr.MessageId

                where m.TemplateId == Template.TicketTemplate
                    && m.SenderProfileId == profileId

                orderby m.MessageId

                select new TicketsCheckVO(
                    m.MessageId,
                    mr.DateReceived))
                .ToArrayAsync(ct);

            return vos;

            Expression<Func<Message, bool>> BuildMessagePredicate(
                int[] ticketIds,
                DateTime? from,
                DateTime? to)
            {
                Expression<Func<Message, bool>> predicate =
                   PredicateBuilder.True<Message>();

                if (ticketIds.Any())
                {
                    predicate = predicate
                        .And(e => this.DbContext.MakeIdsQuery(ticketIds).Any(id => id.Id == e.MessageId));
                }

                DateTime now = DateTime.Now;

                DateTime predicateFrom = from ?? new DateTime(now.Year, 1, 1);
                DateTime predicateTo = to ?? now;

                predicate = predicate
                    .And(e => e.DateSent >= predicateFrom);

                predicate = predicate
                        .And(e => e.DateSent < predicateTo);

                return predicate;
            }

            Expression<Func<MessageRecipient, bool>> BuildMessageRecipientPredicate(
                int? deliveryStatus)
            {
                Expression<Func<MessageRecipient, bool>> predicate =
                   PredicateBuilder.True<MessageRecipient>();

                if (deliveryStatus.HasValue)
                {
                    predicate = deliveryStatus.Value switch
                    {
                        0 => predicate,
                        1 => predicate.And(e => e.DateReceived.HasValue),
                        2 => predicate.And(e => !e.DateReceived.HasValue),
                        _ => throw new ArgumentException(null, nameof(deliveryStatus)),
                    };
                }

                return predicate;
            }
        }
    }
}
