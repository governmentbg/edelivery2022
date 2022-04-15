using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetSummaryAsRecipientVO> GetSummaryAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            var summary = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                where m.MessageId == messageId && mr.ProfileId == profileId

                select new
                {
                    m.MessageSummaryVersion,
                    mr.MessageSummary,
                })
                .SingleAsync(ct);

            GetSummaryAsRecipientVO vo;

            if (summary.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                using IEncryptor encryptorV1 = this.encryptorFactoryV1.CreateEncryptor();
                byte[] decryptyedMessageSummary = encryptorV1.Decrypt(
                        summary.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null"));

                vo = new(
                    $"{messageId}.xml",
                    decryptyedMessageSummary,
                    "application/xml");
            }
            else if (summary.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                vo = new(
                   $"{messageId}.xml",
                   summary.MessageSummary,
                   "application/xml");
            }
            else
            {
                throw new Exception($"Unknown MessageSummaryVersion {summary.MessageSummaryVersion}");
            }

            return vo;
        }
    }
}
