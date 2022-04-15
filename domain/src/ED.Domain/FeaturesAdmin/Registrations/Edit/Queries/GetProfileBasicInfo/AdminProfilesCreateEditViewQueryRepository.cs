using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminRegistrationsEditQueryRepository;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<GetProfileBasicInfoVO> GetProfileBasicInfoAsync(
            int requestRegistrationId,
            CancellationToken ct)
        {
            return await (
                from rr in this.DbContext.Set<RegistrationRequest>()

                join p in this.DbContext.Set<Profile>()
                    on rr.RegisteredProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where rr.RegistrationRequestId == requestRegistrationId

                select new GetProfileBasicInfoVO(
                    p.Id,
                    p.Identifier,
                    tgp.TargetGroupId)
            ).SingleAsync(ct);
        }
    }
}
