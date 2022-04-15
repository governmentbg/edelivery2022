using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetTimestampNroVO> GetTimestampNroAsync(
            string accessCode,
            CancellationToken ct);
    }
}
