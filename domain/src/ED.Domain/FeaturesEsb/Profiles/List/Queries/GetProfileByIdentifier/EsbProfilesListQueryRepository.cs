using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesListQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<GetProfileByIdentifierVO?> GetProfileByIdentifierAsync(
            string identifier,
            CancellationToken ct)
        {
            GetProfileByIdentifierVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where (p.IsActivated || (tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId && p.IsPassive))
                    && EF.Functions.Like(p.Identifier, identifier)

                orderby p.IsActivated

                select new GetProfileByIdentifierVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
