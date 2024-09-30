using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : IEsbMessagesOpenQueryRepository
    {
        public async Task<GetNotificationRecipientsOnDeliveryVO[]> GetNotificationRecipientsOnDeliveryAsync(
            int messageId,
            CancellationToken ct)
        {
            GetNotificationRecipientsOnDeliveryVO[] vos = await (
                from m in this.DbContext.Set<Message>()

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                join lp in this.DbContext.Set<LoginProfile>()
                    on p.Id equals lp.ProfileId

                join l in this.DbContext.Set<Login>()
                    on lp.LoginId equals l.Id

                join lpp in this.DbContext.Set<LoginProfilePermission>()
                    on new { lp.ProfileId, lp.LoginId } equals new { lpp.ProfileId, lpp.LoginId }
                    into lj1
                from lpp in lj1.DefaultIfEmpty()

                where m.MessageId == messageId
                    && p.IsActivated
                    && (lp.EmailNotificationOnDeliveryActive
                        || lp.PhoneNotificationOnDeliveryActive
                        || !string.IsNullOrEmpty(l.PushNotificationsUrl))
                    && (
                        (m.TemplateId == lpp.TemplateId && lpp.Permission == LoginProfilePermissionType.ReadProfileMessageAccess)
                        || lpp.Permission == LoginProfilePermissionType.OwnerAccess
                        || lpp.Permission == LoginProfilePermissionType.FullMessageAccess)

                orderby l.Id

                select new GetNotificationRecipientsOnDeliveryVO(
                    p.Id,
                    p.ElectronicSubjectName,
                    l.Id,
                    l.ElectronicSubjectName,
                    lp.EmailNotificationOnDeliveryActive,
                    lp.Email,
                    lp.PhoneNotificationOnDeliveryActive,
                    lp.Phone,
                    l.PushNotificationsUrl))
                .Distinct()
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
