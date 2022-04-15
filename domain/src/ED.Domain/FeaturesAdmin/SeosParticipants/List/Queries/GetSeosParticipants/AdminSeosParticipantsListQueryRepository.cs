using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminSeosParticipantsListQueryRepository;

namespace ED.Domain
{
    partial class AdminSeosParticipantsListQueryRepository : IAdminSeosParticipantsListQueryRepository
    {
        public async Task<List<GetSeosParticipantsQO>> GetSeosParticipantsAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            var query =
$@"
SELECT 
    arg.Id,
    arg.EIK as Identifier,
    re.Name,
    re.Email,
    re.Phone,
    re.ServiceUrl,
    re.CertificateSN as CertificateNumber
FROM ElectronicDeliverySEOS.dbo.AS4RegisteredEntity arg 
JOIN ElectronicDeliverySEOS.dbo.RegisteredEntity re on arg.EIK = re.EIK
ORDER BY 
    arg.Id ASC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            List<GetSeosParticipantsQO> qos = await this.DbContext.Set<GetSeosParticipantsQO>()
                .FromSqlRaw(query,
                    new SqlParameter("offset", offset),
                    new SqlParameter("limit", limit))
                .AsNoTracking()
                .ToListAsync(ct);

            return qos;
        }
    }
}
