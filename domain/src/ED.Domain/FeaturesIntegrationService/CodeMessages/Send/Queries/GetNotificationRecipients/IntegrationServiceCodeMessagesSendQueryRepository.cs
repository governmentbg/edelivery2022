using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceCodeMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceCodeMessagesSendQueryRepository : IIntegrationServiceCodeMessagesSendQueryRepository
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

                where m.MessageId == messageId

                orderby p.Id

                select new GetNotificationRecipientsVO(
                    p.Id,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone))
                .Distinct()
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
