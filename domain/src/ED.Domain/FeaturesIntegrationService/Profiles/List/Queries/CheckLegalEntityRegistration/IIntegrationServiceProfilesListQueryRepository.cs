using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<CheckLegalEntityRegistrationVO> CheckLegalEntityRegistrationAsync(
            string identifier,
            CancellationToken ct);
    }
}
