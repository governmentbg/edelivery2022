using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetRecipientGroupsCountVO> GetRecipientGroupsCountAsync(
            int profileId,
            CancellationToken ct);
    }
}
