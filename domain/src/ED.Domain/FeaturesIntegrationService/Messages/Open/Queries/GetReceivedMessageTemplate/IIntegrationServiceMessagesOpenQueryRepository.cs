using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<int?> GetReceivedMessageTemplateAsync(
            int messageId,
            CancellationToken ct);
    }
}
