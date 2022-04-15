using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminAdministratorsListQueryRepository;

namespace ED.Domain
{
    partial class AdminAdministratorsListQueryRepository : IAdminAdministratorsListQueryRepository
    {
        public async Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetAllVO> vos = await (
                from ap in this.DbContext.Set<AdminsProfile>()

                join au in this.DbContext.Set<AdminUser>()
                    on ap.Id equals au.Id

                join ap2 in this.DbContext.Set<AdminsProfile>()
                    on ap.CreatedByAdminUserId equals ap2.Id

                join ap3 in this.DbContext.Set<AdminsProfile>()
                    on ap.DisabledByAdminUserId equals ap3.Id
                    into lj1
                from ap3 in lj1.DefaultIfEmpty()

                orderby ap.CreatedOn descending

                select new GetAllVO(
                    ap.Id,
                    $"{ap.FirstName} {ap.LastName}",
                    ap.CreatedOn,
                    $"{ap2.FirstName} {ap2.LastName}",
                    ap3 != null ? $"{ap3.FirstName} {ap3.LastName}" : string.Empty,
                    !au.LockoutEnabled || !au.LockoutEnd.HasValue || au.LockoutEnd!.Value < DateTimeOffset.Now))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
