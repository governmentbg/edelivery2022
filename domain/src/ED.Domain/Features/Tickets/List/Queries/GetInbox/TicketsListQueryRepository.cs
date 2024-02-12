using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITicketsListQueryRepository;

namespace ED.Domain
{
    partial class TicketsListQueryRepository : ITicketsListQueryRepository
    {
        public async Task<TableResultVO<GetInboxQO>> GetInboxAsync(
            int profileId,
            int offset,
            int limit,
            DateTime? from,
            DateTime? to,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    from,
                    to);

            IQueryable<int> countQuery =
                from mr in this.DbContext.Set<MessageRecipient>()
                where mr.ProfileId == profileId
                    && this.DbContext.Set<Ticket>().Any(t => t.MessageId == mr.MessageId)
                select mr.MessageId;

            if (!messagePredicate.IsTrueLambdaExpr())
            {
                countQuery = countQuery.Where(
                    mId =>
                        (from m in this.DbContext.Set<Message>().Where(messagePredicate)
                         where m.MessageId == mId
                         select m)
                        .Any());
            }

            int count = await countQuery.CountAsync(ct);

            if (count == 0)
            {
                return TableResultVO.Empty<GetInboxQO>();
            }

            // Use the DateTime value of the SqlDateTime.MaxValue.
            // EF serializes the SqlDateTime to string which ends up as
            // '31-Dec-99 23:59:59', which is not the intended 9999 year.
            DateTime sqlMaxDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
            DateTime sqlAlmostMaxDate = sqlMaxDate.AddDays(-1);

            string query = $@"
SELECT
    [m].[MessageId],
    [m].[DateSent],
    [p].[ElectronicSubjectName] AS SenderProfileName,
    [m].[Subject],
    [t].[Type],
    [t].[ViolationDate],
    [e].[Status],
    [e].[SeenDate]
FROM [dbo].[Messages] AS [m]
INNER JOIN [dbo].[Tickets] AS [t] ON [m].[MessageId] = [t].[MessageId]
CROSS APPLY (
    SELECT TOP 1
        [ts].[Status],
        [ts].[SeenDate]
    FROM TicketStatuses [ts]
    WHERE [ts].[MessageId] = [t].[MessageId]
    ORDER BY [ts].[TicketStatusId] DESC
) AS [e]
INNER JOIN [dbo].[Profiles] AS [p] ON [m].[SenderProfileId] = [p].[Id]
INNER JOIN [dbo].[MessageRecipients] AS [m0] ON [m].[MessageId] = [m0].[MessageId]
WHERE
    [m0].[ProfileId] = @profileId AND
    [m].[TemplateId] = {Template.TicketTemplate} AND
    {(from.HasValue ? "[m].[DateSent] >= @from AND " : "1=1 AND ")}
    {(to.HasValue ? "[m].[DateSent] < @to AND " : "1=1 AND ")}
    1=1
ORDER BY CASE
    WHEN [m0].[DateReceived] IS NULL THEN @sqlMaxDate
    WHEN [e].[SeenDate] IS NULL THEN @sqlAlmostMaxDate
    ELSE [e].[SeenDate]
END DESC, [m].[DateSent] DESC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY
";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("profileId", SqlDbType.Int) { Value = profileId },
                new SqlParameter("sqlMaxDate", SqlDbType.DateTime2) { Value = sqlMaxDate },
                new SqlParameter("sqlAlmostMaxDate", SqlDbType.DateTime2) { Value = sqlAlmostMaxDate },
                new SqlParameter("offset", SqlDbType.Int) { Value = offset },
                new SqlParameter("limit", SqlDbType.Int) { Value = limit },
            };

            if (from.HasValue)
            {
                parameters.Add(new SqlParameter("from", SqlDbType.DateTime2) { Value = from.Value });
            }

            if (to.HasValue)
            {
                parameters.Add(new SqlParameter("to", SqlDbType.DateTime2) { Value = to.Value.AddDays(1) });
            }

            GetInboxQO[] qos =
                await this.DbContext.Set<GetInboxQO>()
                    .FromSqlRaw(query, parameters.ToArray())
                    .AsNoTracking()
                    .ToArrayAsync(ct);

            return new TableResultVO<GetInboxQO>(qos, count);

            Expression<Func<Message, bool>> BuildMessagePredicate(
                DateTime? fromDate,
                DateTime? toDate)
            {
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>();

                if (fromDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value > fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent < toDate.Value.AddDays(1));
                }

                return predicate;
            }
        }
    }
}
