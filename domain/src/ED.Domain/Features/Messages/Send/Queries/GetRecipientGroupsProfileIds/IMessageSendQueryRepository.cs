using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<int[]> GetRecipientGroupsProfileIdsAsync(
            int[] recipientGroupIds,
            CancellationToken ct);
    }
}
