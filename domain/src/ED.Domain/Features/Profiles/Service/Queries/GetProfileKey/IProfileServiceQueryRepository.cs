using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileServiceQueryRepository
    {
        Task<GetProfileKeyVO> GetProfileKeyAsync(
            int profileKeyId,
            CancellationToken ct);
    }
}
