using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        Task<GetProfilesByIdVO[]> GetProfilesByIdAsync(
            int[] ids,
            CancellationToken ct);
    }
}
