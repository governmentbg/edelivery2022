using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : IEsbMessagesOpenQueryRepository
    {
        public async Task<GetOpenMessageInfoVO> GetOpenMessageInfoAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            GetOpenMessageInfoVO vo = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join l in this.DbContext.Set<Login>()
                    on mr.LoginId equals l.Id

                where mr.MessageId == messageId
                    && mr.ProfileId == profileId

                select new GetOpenMessageInfoVO(
                    mr.DateReceived!.Value,
                    l.Id,
                    l.ElectronicSubjectName))
                .FirstAsync(ct);

            return vo;
        }
    }
}
