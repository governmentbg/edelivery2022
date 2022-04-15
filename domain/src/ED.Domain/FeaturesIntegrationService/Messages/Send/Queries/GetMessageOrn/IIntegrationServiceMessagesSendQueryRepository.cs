using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<string?> GetMessageOrnAsync(
            int messageId,
            int recipientProfileId,
            CancellationToken ct);
    }
}
