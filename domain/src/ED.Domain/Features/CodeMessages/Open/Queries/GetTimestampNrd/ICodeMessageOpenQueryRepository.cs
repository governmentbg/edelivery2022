using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetTimestampNrdVO> GetTimestampNrdAsync(
            string accessCode,
            CancellationToken ct);
    }
}
