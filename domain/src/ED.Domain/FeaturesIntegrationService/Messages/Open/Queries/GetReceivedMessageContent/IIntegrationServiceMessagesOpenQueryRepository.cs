using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        Task<GetReceivedMessageContentVO> GetReceivedMessageContentAsync(
            int profileId,
            int messageId,
            bool firstTimeOpen,
            CancellationToken ct);
    }
}
