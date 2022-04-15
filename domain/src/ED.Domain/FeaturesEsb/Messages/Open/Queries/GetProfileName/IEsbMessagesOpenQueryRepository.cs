using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<string> GetProfileNameAsync(
            int profileId,
            CancellationToken ct);
    }
}
