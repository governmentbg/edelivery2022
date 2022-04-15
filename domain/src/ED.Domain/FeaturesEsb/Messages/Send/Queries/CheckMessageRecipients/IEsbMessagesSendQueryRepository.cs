using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        Task<bool> CheckMessageRecipientsAsync(
            int[] recipientProfileIds,
            int profileId,
            CancellationToken ct);
    }
}
