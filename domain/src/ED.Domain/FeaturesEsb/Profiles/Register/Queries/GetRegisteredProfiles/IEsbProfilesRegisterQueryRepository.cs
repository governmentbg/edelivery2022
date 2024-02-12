using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        Task<TableResultVO<GetRegisteredProfilesVO>> GetRegisteredProfilesAsync(
            string identifier,
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
