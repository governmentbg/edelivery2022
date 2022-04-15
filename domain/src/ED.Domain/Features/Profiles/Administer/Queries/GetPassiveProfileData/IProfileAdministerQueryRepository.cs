using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetPassiveProfileDataVO> GetPassiveProfileDataAsync(
            int loginId,
            CancellationToken ct);
    }
}
