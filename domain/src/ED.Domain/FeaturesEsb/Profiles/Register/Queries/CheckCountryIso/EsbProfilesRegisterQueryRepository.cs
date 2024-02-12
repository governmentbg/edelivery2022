using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<bool> CheckCountryIsoAsync(
            string iso,
            CancellationToken ct)
        {
            bool isValid = await (
                from c in this.DbContext.Set<Country>()

                where EF.Functions.Like(c.CountryISO2, iso)

                select c.CountryISO2)
                .AnyAsync(ct);

            return isValid;
        }
    }
}
