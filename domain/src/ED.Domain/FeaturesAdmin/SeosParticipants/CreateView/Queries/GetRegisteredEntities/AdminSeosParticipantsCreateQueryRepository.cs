using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminSeosParticipantsCreateQueryRepository;

namespace ED.Domain
{
    partial class AdminSeosParticipantsCreateQueryRepository : IAdminSeosParticipantsCreateQueryRepository
    {
        public async Task<List<GetRegisteredEntitiesQO>> GetRegisteredEntitiesAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            var searchTerm = "delivery";
            var query =
$@"
SELECT
    re.Id,
    re.Name,
    re.EIK as Identifier,
    re.ServiceUrl,
    re.CertificateSN as CertificateNumber
FROM ElectronicDeliverySEOS.dbo.RegisteredEntity re
LEFT JOIN ElectronicDeliverySEOS.dbo.AS4RegisteredEntity as4 on re.EIK = as4.EIK
WHERE
    ServiceUrl like '%'+@term+'%' AND
    as4.Id IS NULL
ORDER BY
    re.Id ASC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            List<GetRegisteredEntitiesQO> qos = await this.DbContext.Set<GetRegisteredEntitiesQO>()
                .FromSqlRaw(
                    query,
                    new SqlParameter("term", searchTerm),
                    new SqlParameter("offset", offset),
                    new SqlParameter("limit", limit))
                .AsNoTracking()
                .ToListAsync(ct);

            return qos;
        }
    }
}
