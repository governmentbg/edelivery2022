using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsAsync(
            int messageId,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] vos = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

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
                    && (lp.EmailNotificationActive
                        || lp.PhoneNotificationActive
                        || !string.IsNullOrEmpty(l.PushNotificationsUrl))
                    && (
                        (m.TemplateId == lpp.TemplateId && lpp.Permission == LoginProfilePermissionType.ReadProfileMessageAccess)
                        || lpp.Permission == LoginProfilePermissionType.OwnerAccess
                        || lpp.Permission == LoginProfilePermissionType.FullMessageAccess)

                orderby l.Id

                select new GetNotificationRecipientsVO(
                    p.Id,
                    p.ElectronicSubjectName,
                    l.Id,
                    l.ElectronicSubjectName,
                    lp.EmailNotificationActive,
                    lp.Email,
                    lp.PhoneNotificationActive,
                    lp.Phone,
                    l.PushNotificationsUrl))
                .Distinct()
                .ToArrayAsync(ct);

            if (!vos.Any())
            {
                vos = await (
                    from m in this.DbContext.Set<Message>()

                    join mr in this.DbContext.Set<MessageRecipient>()
                        on m.MessageId equals mr.MessageId

                    join p in this.DbContext.Set<Profile>()
                        on mr.ProfileId equals p.Id

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    where m.MessageId == messageId
                        && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId
                        && p.IsPassive

                    select new GetNotificationRecipientsVO(
                        p.Id,
                        p.ElectronicSubjectName,
                        Login.SystemLoginId,
                        p.ElectronicSubjectName,
                        true,
                        p.EmailAddress,
                        true,
                        p.Phone,
                        null))
                    .Distinct()
                    .ToArrayAsync(ct);
            }

            return vos;
        }
    }
}
