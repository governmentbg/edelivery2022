using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminNomenclaturesListQueryRepository
    {
        Task<GetTargetGroupsByIdVO[]> GetTargetGroupsByIdAsync(
            int[] ids,
            CancellationToken ct);
    }
}
