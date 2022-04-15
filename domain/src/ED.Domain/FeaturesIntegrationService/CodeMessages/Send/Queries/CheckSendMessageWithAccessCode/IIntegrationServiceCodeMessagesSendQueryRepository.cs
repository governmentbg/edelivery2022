using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        Task<bool> CheckSendMessageWithAccessCodeAsync(
            int profileId,
            CancellationToken ct);
    }
}
