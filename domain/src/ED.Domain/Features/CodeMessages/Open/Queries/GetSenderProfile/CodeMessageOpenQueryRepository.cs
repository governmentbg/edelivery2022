using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<GetSenderProfileVO> GetSenderProfileAsync(
            string accessCode,
            CancellationToken ct)
        {
            GetSenderProfileVO vo = await (
                from mac in this.DbContext.Set<MessagesAccessCode>()

                join m in this.DbContext.Set<Message>()
                    on mac.MessageId equals m.MessageId

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                where EF.Functions.Like(mac.AccessCode.ToString(), accessCode)

                select new GetSenderProfileVO(
                    p.Id,
                    p.EmailAddress,
                    p.ElectronicSubjectName,
                    p.Phone,
                    p.Identifier,
                    p.ProfileType))
                .SingleAsync(ct);

            return vo;
        }
    }
}
