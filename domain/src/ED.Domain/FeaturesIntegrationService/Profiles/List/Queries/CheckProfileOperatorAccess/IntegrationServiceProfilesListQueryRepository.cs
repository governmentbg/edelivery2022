using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<CheckProfileOperatorAccessVO> CheckProfileOperatorAccessAsync(
            string profileIdentifier,
            string? operatorIdentifier,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(operatorIdentifier))
            {
                return new CheckProfileOperatorAccessVO(true, null);
            }

            int loginId = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                join p2 in this.DbContext.Set<Profile>()
                    on lp.ProfileId equals p2.Id

                where p.IsActivated
                    && EF.Functions.Like(p.Identifier, operatorIdentifier)
                    && l.IsActive
                    && EF.Functions.Like(p2.Identifier, profileIdentifier)
                    && p2.IsActivated

                select l.Id)
                .FirstOrDefaultAsync(ct);

            return loginId != default(int)
                ? new CheckProfileOperatorAccessVO(true, loginId)
                : new CheckProfileOperatorAccessVO(false, null);
        }
    }
}
