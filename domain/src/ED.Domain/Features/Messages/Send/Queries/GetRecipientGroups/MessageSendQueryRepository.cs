using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<TableResultVO<GetRecipientGroupsQO>> GetRecipientGroupsAsync(
            string? term,
            int profileId,
            int templateId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            // todo: rewrite query for easier maintenance
            string query =
$@"
SELECT
    CASE
        WHEN rg.[IsPublic] = 1 THEN CONCAT(': ', rg.[Name])
        ELSE rg.[Name]
    END AS [Name],
    rg.[RecipientGroupId]
FROM (([dbo].[RecipientGroups] rg
       INNER JOIN [dbo].[RecipientGroupProfiles] rgp ON rg.[RecipientGroupId] = rgp.[RecipientGroupId])
LEFT JOIN ([dbo].[TargetGroupProfiles] tgp
           INNER JOIN [dbo].[TemplateTargetGroups] ttg
               ON ttg.[TargetGroupId] = tgp.[TargetGroupId] AND ttg.[TemplateId] = @templateId AND ttg.[CanReceive] = 1)
       ON rgp.[ProfileId] = tgp.[ProfileId])
LEFT JOIN [dbo].[TemplateProfiles] tp
       ON rgp.[ProfileId] = tp.[ProfileId] AND tp.[TemplateId] = @templateId AND tp.[CanReceive] = 1
WHERE
    rg.[ArchiveDate] IS NULL
    AND (rg.[ProfileId] = @profileId OR rg.[IsPublic] = 1)
    {(!string.IsNullOrEmpty(term) ? "AND rg.[Name] LIKE '%'+@term+'%'" : string.Empty)}
GROUP BY
    rg.[Name],
    rg.[RecipientGroupId],
    rg.[IsPublic]
HAVING
    MIN(ISNULL(tgp.ProfileId, 0) + ISNULL(tp.ProfileId, 0)) > 0";

            List<SqlParameter> parameters = new()
            {
                new("templateId", templateId),
                new("profileId", profileId)
            };

            if (!string.IsNullOrEmpty(term))
            {
                parameters.Add(new("term", term)
                {
                    SqlDbType = System.Data.SqlDbType.NVarChar
                });
            }

            TableResultVO<GetRecipientGroupsQO> qos =
                await this.DbContext.Set<GetRecipientGroupsQO>()
                    .FromSqlRaw(
                        query,
                        parameters.ToArray())
                    .OrderBy(e => e.RecipientGroupId)
                    .AsNoTracking()
                    .ToTableResultAsync(offset, limit, ct);

            return qos;
        }
    }
}
