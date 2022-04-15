using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbStatisticsListQueryRepository;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<GetLastStatisticsVO?> GetLastStatisticsAsync(
            CancellationToken ct)
        {
            GetLastStatisticsVO? vo = await (
                from ms in this.DbContext.Set<MessageStatistics>()

                orderby ms.MonthDate descending

                select new GetLastStatisticsVO(
                    ms.MonthDate,
                    ms.Month,
                    ms.MessagesSent,
                    ms.MessagesReceived))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
