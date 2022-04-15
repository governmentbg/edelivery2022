using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<int> GetProfileByLoginIdAsync(
            int loginId,
            CancellationToken ct);
    }
}
