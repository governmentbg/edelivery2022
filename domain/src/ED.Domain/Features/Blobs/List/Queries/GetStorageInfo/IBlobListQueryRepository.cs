using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        Task<GetStorageInfoVO> GetStorageInfoAsync(
            int profileId,
            CancellationToken ct);
    }
}
