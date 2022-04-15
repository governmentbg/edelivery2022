using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetPdfAsRecipientVO> GetPdfAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
