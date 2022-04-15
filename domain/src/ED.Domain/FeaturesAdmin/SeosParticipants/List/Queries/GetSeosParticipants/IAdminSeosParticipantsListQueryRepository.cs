using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminSeosParticipantsListQueryRepository
    {
        Task<List<GetSeosParticipantsQO>> GetSeosParticipantsAsync(
            int offset,
            int limit,
            CancellationToken ct);
    }
}
