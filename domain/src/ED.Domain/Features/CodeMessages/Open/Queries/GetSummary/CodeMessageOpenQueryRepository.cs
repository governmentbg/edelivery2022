using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<GetSummaryVO> GetSummaryAsync(
            string accessCode,
            CancellationToken ct)
        {
            var summary = await (
                from mac in this.DbContext.Set<MessagesAccessCode>()

                join m in this.DbContext.Set<Message>()
                    on mac.MessageId equals m.MessageId

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                where EF.Functions.Like(mac.AccessCode.ToString(), accessCode)

                select new
                {
                    m.MessageId,
                    m.MessageSummaryVersion,
                    mr.MessageSummary,
                })
                .FirstAsync(ct);

            GetSummaryVO vo;

            if (summary.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                using IEncryptor encryptorV1 = this.encryptorFactoryV1.CreateEncryptor();
                byte[] decryptyedMessageSummary = encryptorV1.Decrypt(
                        summary.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null"));

                vo = new(
                    $"{summary.MessageId}.xml",
                    decryptyedMessageSummary,
                    "application/xml");
            }
            else if (summary.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                vo = new(
                   $"{summary.MessageId}.xml",
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
