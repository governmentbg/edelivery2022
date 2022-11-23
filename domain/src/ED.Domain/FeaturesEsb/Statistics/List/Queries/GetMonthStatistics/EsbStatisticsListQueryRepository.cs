using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbStatisticsListQueryRepository;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<GetMonthStatisticsVO?> GetMonthStatisticsAsync(
            DateTime month,
            CancellationToken ct)
        {
            GetMonthStatisticsVO? vo = await (
                from ms in this.DbContext.Set<MessageStatistics>()

                where ms.MonthDate == month

                orderby ms.MonthDate descending

                select new GetMonthStatisticsVO(
                    ms.MonthDate,
                    ms.Month,
                    ms.MessagesSent,
                    ms.MessagesReceived))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
