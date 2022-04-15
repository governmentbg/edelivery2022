using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<bool> GetExistingLoginAsync(
            Guid loginSubjectId,
            CancellationToken ct);
    }
}
