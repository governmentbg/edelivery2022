using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<GetProfileNamesVO[]> GetProfileNamesAsync(
            int[] profileIds,
            CancellationToken ct);
    }
}
