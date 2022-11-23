using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<CheckProfileOperatorAccessVO> CheckProfileOperatorAccessAsync(
            string profileIdentifier,
            string? operatorIdentifier,
            CancellationToken ct);
    }
}
