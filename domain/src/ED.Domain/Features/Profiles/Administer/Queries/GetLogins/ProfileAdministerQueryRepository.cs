using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetLoginsVO[]> GetLoginsAsync(
            int profileId,
            CancellationToken ct)
        {
            GetLoginsVO[] vos = await (
                from lp in this.DbContext.Set<LoginProfile>()

                join l1 in this.DbContext.Set<Login>()
                    on lp.LoginId equals l1.Id

                join l2 in this.DbContext.Set<Login>()
                    on lp.AccessGrantedBy equals l2.Id

                where lp.ProfileId == profileId

                orderby lp.DateAccessGranted descending

                select new GetLoginsVO(
                    lp.LoginId,
                    l1.ElectronicSubjectName,
                    l2.ElectronicSubjectName,
                    lp.DateAccessGranted,
                    lp.IsDefault))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
