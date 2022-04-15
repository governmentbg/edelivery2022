using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfileCreateEditViewQueryRepository
    {
        Task<GetAdminProfileVO> GetAdminProfileAsync(
            int id,
            CancellationToken ct);
    }
}
