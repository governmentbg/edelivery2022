using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<bool> HasActiveProfileAsync(
            int profileId,
            string identifier,
            int targetGroupId,
            CancellationToken ct);
    }
}
