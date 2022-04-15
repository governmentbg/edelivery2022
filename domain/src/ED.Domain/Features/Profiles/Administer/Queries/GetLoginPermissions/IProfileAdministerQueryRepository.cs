using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetLoginPermissionsVO> GetLoginPermissionsAsync(
            int profileId,
            int loginId,
            CancellationToken ct);
    }
}
