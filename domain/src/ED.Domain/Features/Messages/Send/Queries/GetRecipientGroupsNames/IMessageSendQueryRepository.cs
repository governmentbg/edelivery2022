using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<string[]> GetRecipientGroupsNamesAsync(
            int[] recipientGroupIds,
            CancellationToken ct);
    }
}
