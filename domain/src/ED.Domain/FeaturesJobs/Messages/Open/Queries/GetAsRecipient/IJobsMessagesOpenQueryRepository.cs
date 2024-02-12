using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IJobsMessagesOpenQueryRepository
    {
        Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
