using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetRecipientGroupVO> GetRecipientGroupAsync(
            int recipientGroupId,
            int profileId,
            CancellationToken ct);
    }
}
