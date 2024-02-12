using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface ITicketsListQueryRepository
    {
        [Keyless]
        public record GetNewTicketsCountQO(
            int ProfileId,
            int Count);
    }
}
