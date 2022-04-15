using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetLoginProfileNotificationSettingsVO> GetLoginProfileNotificationSettingsAsync(
            int loginId,
            int profileId,
            CancellationToken ct)
        {
            GetLoginProfileNotificationSettingsVO vo = await (
                from lp in this.DbContext.Set<LoginProfile>()

                where lp.LoginId == loginId
                    && lp.ProfileId == profileId

                select new GetLoginProfileNotificationSettingsVO(
                    lp.Email,
                    lp.Phone,
                    lp.EmailNotificationActive,
                    lp.EmailNotificationOnDeliveryActive,
                    lp.SmsNotificationActive,
                    lp.SmsNotificationOnDeliveryActive,
                    lp.ViberNotificationActive,
                    lp.ViberNotificationOnDeliveryActive))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
