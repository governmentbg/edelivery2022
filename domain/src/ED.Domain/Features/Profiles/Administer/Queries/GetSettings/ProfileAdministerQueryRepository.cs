using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetSettingsVO> GetSettingsAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            GetSettingsVO vo = await (
                from lp in this.DbContext.Set<LoginProfile>()

                where lp.ProfileId == profileId
                    && lp.LoginId == loginId

                select new GetSettingsVO(
                    lp.EmailNotificationActive,
                    lp.EmailNotificationOnDeliveryActive,
                    lp.PhoneNotificationActive,
                    lp.PhoneNotificationOnDeliveryActive,
                    lp.Email,
                    lp.Phone))
                .FirstAsync(ct);

            return vo;
        }
    }
}
