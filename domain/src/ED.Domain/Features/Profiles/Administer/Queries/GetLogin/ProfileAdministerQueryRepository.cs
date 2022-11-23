using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetLoginVO> GetLoginAsync(
            Guid profileGuid,
            CancellationToken ct)
        {
            GetLoginVO vo = await (
                from l in this.DbContext.Set<Login>()

                where l.ElectronicSubjectId == profileGuid

                select new GetLoginVO(l.Id))
                .SingleAsync(ct);

            return vo;
        }
    }
}
