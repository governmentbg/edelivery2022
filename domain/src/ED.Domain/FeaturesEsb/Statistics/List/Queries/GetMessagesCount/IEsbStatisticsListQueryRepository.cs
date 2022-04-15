using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        Task<GetMessagesCountVO> GetMessagesCountAsync(
            DateTime month,
            CancellationToken ct);
    }
}
