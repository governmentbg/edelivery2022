using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        private const string NrdExtension = "NRD.tsr";

        public async Task<GetTimestampNrdVO> GetTimestampNrdAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            var timestampInfo = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join p2 in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p2.Id

                where m.MessageId == messageId

                select new
                {
                    m.DateCreated,
                    SenderProfileGuid = p1.ElectronicSubjectId,
                    RecipientProfileGuid = p2.ElectronicSubjectId,
                    mr.Timestamp
                })
                .SingleAsync(ct);

            string fileName = $"{timestampInfo.DateCreated:yyyyMMdd}_{timestampInfo.SenderProfileGuid}_{timestampInfo.RecipientProfileGuid}_{NrdExtension}";

            GetTimestampNrdVO vo = new(fileName, timestampInfo.Timestamp);

            return vo;
        }
    }
}
