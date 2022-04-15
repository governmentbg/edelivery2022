using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<GetMessageSenderVO?> GetMessageSenderAsync(
            int messageId,
            int recipientProfileId,
            CancellationToken ct);
    }
}
