using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<CheckProfileRegistrationVO> CheckProfileRegistrationAsync(
            string identifier,
            CancellationToken ct)
        {
            CheckProfileRegistrationVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && !p.IsPassive

                orderby p.IsActivated descending

                select new CheckProfileRegistrationVO(
                    true,
                    identifier,
                    p.ElectronicSubjectId.ToString(),
                    p.ElectronicSubjectName,
                    p.IsActivated,
                    p.EmailAddress,
                    p.Phone,
                    tgp.TargetGroupId
                ))
                .FirstOrDefaultAsync(ct);

            return vo ?? CheckProfileRegistrationVO.Empty(identifier);
        }
    }
}
