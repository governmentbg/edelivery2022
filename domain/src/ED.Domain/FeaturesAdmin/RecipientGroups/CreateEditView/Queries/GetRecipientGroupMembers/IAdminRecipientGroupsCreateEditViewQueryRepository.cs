using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        Task<GetRecipientGroupMembersVO[]> GetRecipientGroupMembersAsync(
            int recipientGroupId,
            CancellationToken ct);
    }
}
