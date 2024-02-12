using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageListQueryRepository;

namespace ED.Domain
{
    partial class MessageListQueryRepository
    {
        public async Task<TableResultVO<GetOutboxQO>> GetOutboxAsync(
            int profileId,
            int offset,
            int limit,
            string? subject,
            string? profile,
            DateTime? from,
            DateTime? to,
            string? rnu,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    profileId,
                    subject,
                    profile,
                    from,
                    to,
                    rnu);

            int count = await this.DbContext.Set<Message>()
                .Where(messagePredicate)
                .CountAsync(ct);

            if (count == 0)
            {
                return TableResultVO.Empty<GetOutboxQO>();
            }

            string query = $@"
WITH RecipientCounts AS (
    SELECT
        [MessageId],
        COUNT(CASE WHEN [DateReceived] IS NOT NULL THEN 1 END) AS NumberOfRecipients,
        COUNT(*) AS NumberOfTotalRecipients
    FROM [dbo].[MessageRecipients]
    GROUP BY [MessageId]
)
SELECT
    [m].[MessageId],
    [m].[DateSent],
    [p].[ElectronicSubjectName] AS SenderProfileName,
    [l].[ElectronicSubjectName] AS SenderLoginName,
    [m].[RecipientsAsText] AS Recipients,
    [m].[SubjectExtended] AS Subject,
    [m].[ForwardStatusId],
    [t].[Name] AS TemplateName,
    [rc].NumberOfRecipients,
    [rc].NumberOfTotalRecipients,
    [m].[Rnu]
FROM [dbo].[Messages] AS [m]
INNER JOIN [dbo].[Profiles] AS [p] ON [m].[SenderProfileId] = [p].[Id]
INNER JOIN [dbo].[Logins] AS [l] ON [m].[SenderLoginId] = [l].[Id]
INNER JOIN [dbo].[Templates] AS [t] ON [m].[TemplateId] = [t].[TemplateId]
LEFT JOIN RecipientCounts AS [rc] ON [m].[MessageId] = [rc].[MessageId]
WHERE
    [m].[SenderProfileId] = @profileId AND
    {(!string.IsNullOrEmpty(subject) ? "[m].[Subject] LIKE @subject AND " : "1=1 AND ")}
    {(!string.IsNullOrEmpty(profile) ? "[m].[RecipientsAsText] LIKE @profile AND " : "1=1 AND ")}
    {(from.HasValue ? "[m].[DateSent] >= @from AND " : "1=1 AND ")}
    {(to.HasValue ? "[m].[DateSent] < @to AND " : "1=1 AND ")}
    {(!string.IsNullOrEmpty(rnu) ? $"[m].[Rnu] LIKE @rnu AND " : "1=1 AND ")}
    1=1
ORDER BY [m].[DateSent] DESC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY
";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("profileId", SqlDbType.Int) { Value = profileId },
                new SqlParameter("offset", SqlDbType.Int) { Value = offset },
                new SqlParameter("limit", SqlDbType.Int) { Value = limit },
            };

            if (!string.IsNullOrEmpty(subject))
            {
                parameters.Add(new SqlParameter("subject", SqlDbType.NVarChar) { Value = $"%{subject}%" });
            }

            if (!string.IsNullOrEmpty(profile))
            {
                parameters.Add(new SqlParameter("profile", SqlDbType.NVarChar) { Value = $"%{profile}%" });
            }

            if (from.HasValue)
            {
                parameters.Add(new SqlParameter("from", SqlDbType.DateTime2) { Value = from.Value });
            }

            if (to.HasValue)
            {
                parameters.Add(new SqlParameter("to", SqlDbType.DateTime2) { Value = to.Value.AddDays(1) });
            }

            if (!string.IsNullOrEmpty(rnu))
            {
                parameters.Add(new SqlParameter("rnu", SqlDbType.NVarChar) { Value = rnu });
            }

            GetOutboxQO[] qos =
                await this.DbContext.Set<GetOutboxQO>()
                    .FromSqlRaw(query, parameters.ToArray())
                    .AsNoTracking()
                    .ToArrayAsync(ct);

            return new TableResultVO<GetOutboxQO>(qos, count);

            static Expression<Func<Message, bool>> BuildMessagePredicate(
                int profileId,
                string? subject,
                string? profile,
                DateTime? from,
                DateTime? to,
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

                if (from.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value > from.Value);
                }

                if (to.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value < to.Value.AddDays(1));
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
