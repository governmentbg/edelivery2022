using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<GetProfileInfoVO> GetProfileInfoAsync(
            string subjectId,
            CancellationToken ct);
    }
}
