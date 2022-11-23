using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<GetMessageReplyInfoVO> GetMessageReplyInfoAsync(
            int messageId,
            CancellationToken ct);
    }
}
