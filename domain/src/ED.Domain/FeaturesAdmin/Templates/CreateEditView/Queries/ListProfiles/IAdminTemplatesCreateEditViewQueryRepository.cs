using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        Task<ListProfilesVO[]> ListProfilesAsync(
            string term,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
