using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminAdministratorsCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminAdministratorsCreateEditViewQueryRepository : IAdminAdministratorsCreateEditViewQueryRepository
    {
        public async Task<GetAdministratorVO> GetAdministratorAsync(
            int id,
            CancellationToken ct)
        {
            GetAdministratorVO vo = await (
                from au in this.DbContext.Set<AdminUser>()

                join ap in this.DbContext.Set<AdminsProfile>()
                    on au.Id equals ap.Id

                join ap2 in this.DbContext.Set<AdminsProfile>()
                    on ap.CreatedByAdminUserId equals ap2.Id

                join ap3 in this.DbContext.Set<AdminsProfile>()
                    on ap.DisabledByAdminUserId equals ap3.Id
                    into lj1
                from ap3 in lj1.DefaultIfEmpty()

                where au.Id == id

                select new GetAdministratorVO(
                    au.Id,
                    ap.FirstName,
                    ap.MiddleName,
                    ap.LastName,
                    ap.EGN,
                    au.PhoneNumber!,
                    au.Email!,
                    au.NormalizedUserName!,
                    !au.LockoutEnabled || !au.LockoutEnd.HasValue || au.LockoutEnd!.Value < DateTimeOffset.Now,
                    ap.CreatedOn,
                    $"{ap2.FirstName} {ap2.LastName}",
                    ap.DisabledOn,
                    ap3 != null ? $"{ap3.FirstName} {ap3.LastName}" : string.Empty,
                    ap.DisableReason != null
                        ? ap.DisableReason!
                        : string.Empty))
                .SingleAsync(ct);

            return vo;
        }
    }
}
