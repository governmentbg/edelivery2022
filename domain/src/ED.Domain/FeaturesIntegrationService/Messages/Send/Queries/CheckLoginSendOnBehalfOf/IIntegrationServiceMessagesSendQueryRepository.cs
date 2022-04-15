using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<bool> CheckLoginSendOnBehalfOfAsync(
            int loginId,
            CancellationToken ct);
    }
}
