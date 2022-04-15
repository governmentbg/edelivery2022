using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetLoginInfoVO> GetLoginInfoAsync(
            int loginId,
            Guid profileSubjectId,
            CancellationToken ct);
    }
}
