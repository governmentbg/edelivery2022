using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        Task<string> GetProfileIdentifierAsync(
            int profileId,
            CancellationToken ct);
    }
}
