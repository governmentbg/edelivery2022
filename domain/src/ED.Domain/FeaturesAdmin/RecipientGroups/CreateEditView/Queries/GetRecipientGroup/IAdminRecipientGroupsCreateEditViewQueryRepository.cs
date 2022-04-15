using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        Task<GetRecipientGroupVO> GetRecipientGroupAsync(
            int recipientGroupId,
            CancellationToken ct);
    }
}
