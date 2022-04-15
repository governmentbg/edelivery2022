using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<GetProfileMessageRoleVO?> GetProfileMessageRoleAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            GetProfileMessageRoleVO vo = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on new { m.MessageId, ProfileId = profileId } equals new { mr.MessageId, mr.ProfileId }
                    into lj1
                from mr in lj1.DefaultIfEmpty()

                where m.MessageId == messageId

                select new GetProfileMessageRoleVO(
                    m.SenderProfileId == profileId,
                    mr != null))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
