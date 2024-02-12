using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IJobsMessagesOpenHORepository
    {
        Task<string> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
