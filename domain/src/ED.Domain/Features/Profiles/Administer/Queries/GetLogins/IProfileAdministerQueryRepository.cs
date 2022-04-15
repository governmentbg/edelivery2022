using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetLoginsVO[]> GetLoginsAsync(
            int profileId,
            CancellationToken ct);
    }
}
