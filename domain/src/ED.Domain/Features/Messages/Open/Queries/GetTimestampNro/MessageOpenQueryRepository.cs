using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        private const string NroExtension = "NRO.tsr";

        public async Task<GetTimestampNroVO> GetTimestampNroAsync(
            int messageId,
            CancellationToken ct)
        {
            var timestampInfo = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                where m.MessageId == messageId

                select new
                {
                    m.MessageId,
                    m.DateCreated,
                    SenderProfileGuid = p1.ElectronicSubjectId,
                    Timestamp = m.TimeStampNRO
                })
                .SingleAsync(ct);

            string fileName = $"{timestampInfo.DateCreated:yyyyMMdd}_{timestampInfo.SenderProfileGuid}_{timestampInfo.MessageId}_{NroExtension}";

            GetTimestampNroVO vo = new(fileName, timestampInfo.Timestamp);

            return vo;
        }
    }
}
