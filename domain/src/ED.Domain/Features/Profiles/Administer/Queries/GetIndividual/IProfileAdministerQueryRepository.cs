using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetIndividualVO> GetIndividualAsync(
            int profileId,
            CancellationToken ct);
    }
}
