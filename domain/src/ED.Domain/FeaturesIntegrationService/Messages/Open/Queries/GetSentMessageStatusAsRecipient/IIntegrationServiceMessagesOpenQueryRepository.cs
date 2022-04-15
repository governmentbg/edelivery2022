using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetSentMessageStatusAsRecipientVO> GetSentMessageStatusAsRecipientAsync(
            int profileId,
            int messageId,
            bool firstTimeOpen,
            CancellationToken ct);
    }
}
