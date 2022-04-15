using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminSeosParticipantsCreateQueryRepository
    {
        Task<List<GetRegisteredEntitiesQO>> GetRegisteredEntitiesAsync(
            int offset,
            int limit,
            CancellationToken ct);
    }
}
