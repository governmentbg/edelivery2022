using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetAllowedTemplatesVO[]> GetAllowedTemplatesAsync(
            int profileId,
            CancellationToken ct);
    }
}
