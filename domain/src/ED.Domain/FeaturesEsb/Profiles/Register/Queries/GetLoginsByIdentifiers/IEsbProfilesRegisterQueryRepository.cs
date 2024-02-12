using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        Task<GetLoginsByIdentifiersVO[]> GetLoginsByIdentifiersAsync(
            string[] identifiers,
            CancellationToken ct);
    }
}
