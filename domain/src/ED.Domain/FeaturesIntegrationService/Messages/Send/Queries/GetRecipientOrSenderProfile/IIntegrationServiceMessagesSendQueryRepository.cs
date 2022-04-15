using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<GetRecipientOrSenderProfileVO?> GetRecipientOrSenderProfileAsync(
            string identifier,
            int[] targetGroupIds,
            CancellationToken ct);
    }
}
