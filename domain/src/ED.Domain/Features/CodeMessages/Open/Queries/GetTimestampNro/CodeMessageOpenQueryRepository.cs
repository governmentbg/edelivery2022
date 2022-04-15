using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        private const string NroExtension = "NRO.tsr";

        public async Task<GetTimestampNroVO> GetTimestampNroAsync(
            string accessCode,
            CancellationToken ct)
        {
            var timestampInfo = await (
                from mac in this.DbContext.Set<MessagesAccessCode>()

                join m in this.DbContext.Set<Message>()
                    on mac.MessageId equals m.MessageId

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                where EF.Functions.Like(mac.AccessCode.ToString(), accessCode)

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
