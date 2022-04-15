using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetSummaryVO> GetSummaryAsync(
            string accesCode,
            CancellationToken ct);
    }
}
