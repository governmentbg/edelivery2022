using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetLoginInfoVO> GetLoginInfoAsync(
            int loginId,
            Guid profileSubjectId,
            CancellationToken ct)
        {
            GetLoginInfoVO vo = await (
                from l in this.DbContext.Set<Login>()

                where l.Id == loginId

                select new GetLoginInfoVO(
                    l.ElectronicSubjectId,
                    l.ElectronicSubjectName,
                    l.Email,
                    l.PhoneNumber,
                    l.ElectronicSubjectId == profileSubjectId))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
