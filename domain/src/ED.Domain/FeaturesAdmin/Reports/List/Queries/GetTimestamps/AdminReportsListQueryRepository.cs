using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        public async Task<GetTimestampsVO> GetTimestampsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method} ({adminUserId}, \"{fromDate}\", \"{toDate}\") called",
                nameof(GetTimestampsAsync),
                adminUserId,
                fromDate,
                toDate);

            int countSuccess = await (
               from tra in this.DbContext.Set<TimestampRequestAudit>()

               where tra.DateSent >= fromDate
                     && tra.DateSent < toDate.AddDays(1)
                     && tra.Status == TimestampRequestAuditStatus.Success

               select tra)
               .CountAsync(ct);

            int countError = await (
               from tra in this.DbContext.Set<TimestampRequestAudit>()

               where tra.DateSent >= fromDate
                     && tra.DateSent < toDate.AddDays(1)
                     && tra.Status == TimestampRequestAuditStatus.Error

               select tra)
               .CountAsync(ct);

            return new(countSuccess, countError);
        }
    }
}
