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
        public async Task<GetNewTicketsCountQO[]> GetNewTicketsCountAsync(
            int loginId,
            CancellationToken ct)
        {
            string query = $@"
SELECT
    [p].[Id] AS ProfileId,
    COUNT(*) AS [Count]
FROM [dbo].[Profiles] AS [p]
INNER JOIN [dbo].[LoginsProfiles] AS [l] ON [p].[Id] = [l].[ProfileId]
INNER JOIN [dbo].[MessageRecipients] AS [m] ON [p].[Id] = [m].[ProfileId]
INNER JOIN [dbo].[Messages] AS [m0] ON [m].[MessageId] = [m0].[MessageId]
INNER JOIN [dbo].[Tickets] AS [t] ON [m0].[MessageId] = [t].[MessageId]
CROSS APPLY (
    SELECT TOP 1
        [ts].[Status],
        [ts].[SeenDate]
    FROM TicketStatuses [ts]
    WHERE [ts].[MessageId] = [t].[MessageId]
    ORDER BY [ts].[TicketStatusId] DESC
) AS [e]
WHERE ((([p].[IsActivated] = CAST(1 AS bit)) AND ([l].[LoginId] = @loginId)) AND (([m].[DateReceived] IS NULL) OR ([e].[SeenDate] IS NULL))) AND ([m0].[TemplateId] = {Template.TicketTemplate})
GROUP BY [p].[Id]
";

            SqlParameter[] parameters = new[]
            {
                new SqlParameter("loginId", SqlDbType.Int) { Value = loginId },
            };

            GetNewTicketsCountQO[] qos =
                await this.DbContext.Set<GetNewTicketsCountQO>()
                    .FromSqlRaw(query, parameters)
                    .AsNoTracking()
                    .ToArrayAsync(ct);

            return qos;
        }
    }
}
