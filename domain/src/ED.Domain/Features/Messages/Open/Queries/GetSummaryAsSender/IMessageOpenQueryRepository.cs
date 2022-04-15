using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetSummaryAsSenderVO> GetSummaryAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
