using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetSettingsVO> GetSettingsAsync(
            int profileId,
            int loginId,
            CancellationToken ct);
    }
}
