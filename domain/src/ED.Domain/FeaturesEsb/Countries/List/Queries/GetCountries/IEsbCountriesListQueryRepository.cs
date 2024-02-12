using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbCountriesListQueryRepository
    {
        Task<TableResultVO<GetCountriesVO>> GetCountriesAsync(
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
