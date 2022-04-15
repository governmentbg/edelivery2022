using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetLoginInfoVO> GetLoginInfoAsync(
            int loginId,
            Guid profileSubjectId,
            CancellationToken ct);
    }
}
