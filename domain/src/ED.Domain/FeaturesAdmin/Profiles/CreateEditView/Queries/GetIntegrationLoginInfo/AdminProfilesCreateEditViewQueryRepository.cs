using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetIntegrationLoginInfoVO?> GetIntegrationLoginInfoAsync(
            int profileId,
            CancellationToken ct)
        {
            GetIntegrationLoginInfoVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId
                    into lj1
                from l in lj1.DefaultIfEmpty()

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                where p.Id == profileId

                select new GetIntegrationLoginInfoVO(
                    l.CertificateThumbprint,
                    l.PushNotificationsUrl,
                    l.CanSendOnBehalfOf,
                    lp.PhoneNotificationActive,
                    lp.PhoneNotificationOnDeliveryActive,
                    lp.EmailNotificationActive,
                    lp.EmailNotificationOnDeliveryActive,
                    lp.Email,
                    lp.Phone))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
