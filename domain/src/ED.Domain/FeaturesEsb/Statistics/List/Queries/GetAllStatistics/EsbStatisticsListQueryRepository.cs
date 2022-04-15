using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbStatisticsListQueryRepository;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<GetAllStatisticsVO[]> GetAllStatisticsAsync(
            CancellationToken ct)
        {
            GetAllStatisticsVO[] vos = await (
                from ms in this.DbContext.Set<MessageStatistics>()

                orderby ms.MonthDate ascending

                select new GetAllStatisticsVO(
                    ms.MonthDate,
                    ms.Month,
                    ms.MessagesSent,
                    ms.MessagesReceived))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
