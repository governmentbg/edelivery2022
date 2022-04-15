using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<string> GetLoginNameAsync(
            int loginId,
            CancellationToken ct)
        {
            string name = await (
                from l in this.DbContext.Set<Login>()

                where l.Id == loginId

                select l.ElectronicSubjectName)
                .SingleAsync(ct);

            return name;
        }
    }
}
