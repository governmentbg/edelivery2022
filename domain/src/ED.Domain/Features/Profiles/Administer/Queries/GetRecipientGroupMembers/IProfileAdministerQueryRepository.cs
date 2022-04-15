using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<TableResultVO<GetRecipientGroupMembersVO>> GetRecipientGroupMembersAsync(
            int recipientGroupId,
            CancellationToken ct);
    }
}
