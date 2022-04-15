using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<GetLoginByIdentifierVO?> GetLoginByIdentifierAsync(
            string identifier,
            CancellationToken ct);
    }
}
