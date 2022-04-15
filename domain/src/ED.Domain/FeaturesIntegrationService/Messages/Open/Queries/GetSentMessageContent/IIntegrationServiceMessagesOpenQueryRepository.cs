using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetSentMessageContentVO> GetSentMessageContentAsync(
            int profileId,
            int messageId,
            CancellationToken ct);
    }
}
