using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbTicketsSendQueryRepository;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsAsync(
            bool isRecipientIndividual,
            int messageId,
            CancellationToken ct)
        {
            return isRecipientIndividual
                ? await this.GetNotificationRecipientsForIndividualAsync(messageId, ct)
                : await this.GetNotificationRecipientsForLegalEntityAsync(messageId, ct);
        }

        private async Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsForIndividualAsync(
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
                    into j1
                from lp in j1.DefaultIfEmpty()

                where m.MessageId == messageId

                select new GetNotificationRecipientsVO(
                    p.Id,
                    lp != null ? lp.LoginId : null,
                    p.IsPassive || lp.EmailNotificationActive,
                    p.IsPassive ? p.EmailAddress : lp.Email,
                    p.IsPassive || lp.SmsNotificationActive,
                    p.IsPassive || lp.ViberNotificationActive,
                    p.IsPassive ? p.Phone : lp.Phone))
                .Distinct()
                .ToArrayAsync(ct);

            return vos;
        }

        private async Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsForLegalEntityAsync(
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
                    into lj1
                from lp in lj1.DefaultIfEmpty()

                join lpp in this.DbContext.Set<LoginProfilePermission>()
                    on new { lp.ProfileId, lp.LoginId } equals new { lpp.ProfileId, lpp.LoginId }
                    into lj2
                from lpp in lj2.DefaultIfEmpty()

                where m.MessageId == messageId
                    && p.IsActivated
                    && (
                        lpp == null
                        || (m.TemplateId == lpp.TemplateId && lpp.Permission == LoginProfilePermissionType.ReadProfileMessageAccess)
                        || lpp.Permission == LoginProfilePermissionType.OwnerAccess
                        || lpp.Permission == LoginProfilePermissionType.FullMessageAccess
                        )

                select new GetNotificationRecipientsVO(
                    p.Id,
                    lp != null ? lp.LoginId : null,
                    lp != null && lp.EmailNotificationActive,
                    lp != null ? lp.Email : string.Empty,
                    lp != null && lp.SmsNotificationActive,
                    lp != null && lp.ViberNotificationActive,
                    lp != null ? lp.Phone : string.Empty))
                .Distinct()
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
