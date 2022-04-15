using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<GetAuthenticationInfoVO?> GetAuthenticationInfoAsync(
            string certificateThumbprint,
            string? operatorIdentifier,
            CancellationToken ct);
    }
}
