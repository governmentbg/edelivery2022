using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<bool> HasExistingGroupName(
            string groupName,
            int profileId,
            CancellationToken ct);
    }
}
