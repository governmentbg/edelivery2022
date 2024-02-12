using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IEsbCountriesListQueryRepository;

namespace ED.Domain
{
    partial class EsbCountriesListQueryRepository : IEsbCountriesListQueryRepository
    {
        public async Task<TableResultVO<GetCountriesVO>> GetCountriesAsync(
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            TableResultVO<GetCountriesVO> vos = await (
                from c in this.DbContext.Set<Country>()

                orderby c.CountryISO2

                select new GetCountriesVO(
                    c.CountryISO2,
                    c.Name))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
