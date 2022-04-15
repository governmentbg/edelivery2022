using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetSummaryAsRecipientVO> GetSummaryAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
