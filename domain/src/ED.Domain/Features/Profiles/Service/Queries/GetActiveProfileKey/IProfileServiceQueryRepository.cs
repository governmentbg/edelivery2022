using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileServiceQueryRepository
    {
        Task<GetActiveProfileKeyVO?> GetActiveProfileKeyAsync(
            int profileId,
            CancellationToken ct);
    }
}
