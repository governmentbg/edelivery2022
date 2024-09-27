using System;
using System.Collections.Generic;
using System.Data;
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
            TicketStatusStatus? status,
            CancellationToken ct)
        {
            string countQuery = $@"
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
    {(status.HasValue ? "[e].[Status] = @status AND " : "1=1 AND ")}
    1=1
";

            List<SqlParameter> countParameters = new()
            {
                new SqlParameter("profileId", SqlDbType.Int) { Value = profileId },
            };

            if (from.HasValue)
            {
                countParameters.Add(new SqlParameter("from", SqlDbType.DateTime2) { Value = from.Value });
            }

            if (to.HasValue)
            {
                countParameters.Add(new SqlParameter("to", SqlDbType.DateTime2) { Value = to.Value.AddDays(1) });
            }

            if (status.HasValue)
            {
                countParameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = (int)status.Value });
            }

            int count =
                await this.DbContext.Set<GetInboxQO>()
                    .FromSqlRaw(countQuery, countParameters.ToArray())
                    .AsNoTracking()
                    .CountAsync(ct);

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
    {(status.HasValue ? "[e].[Status] = @status AND " : "1=1 AND ")}
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

            if (status.HasValue)
            {
                parameters.Add(new SqlParameter("status", SqlDbType.Int) { Value = (int)status.Value });
            }

            GetInboxQO[] qos =
                await this.DbContext.Set<GetInboxQO>()
                    .FromSqlRaw(query, parameters.ToArray())
                    .AsNoTracking()
                    .ToArrayAsync(ct);

            return new TableResultVO<GetInboxQO>(qos, count);
        }
    }
}
