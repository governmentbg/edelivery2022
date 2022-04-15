using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<CheckIndividualRegistrationVO> CheckIndividualRegistrationAsync(
            string identifier,
            CancellationToken ct);
    }
}
