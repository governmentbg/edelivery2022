using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetLoginVO> GetLoginAsync(
            Guid profileGuid,
            CancellationToken ct);
    }
}
