using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetIndividualByIdentifierVO?> GetIndividualByIdentifierAsync(
            string identifier,
            CancellationToken ct);
    }
}
