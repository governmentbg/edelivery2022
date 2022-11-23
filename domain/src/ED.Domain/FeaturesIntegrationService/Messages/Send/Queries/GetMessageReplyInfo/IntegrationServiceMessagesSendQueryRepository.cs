using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<GetMessageReplyInfoVO> GetMessageReplyInfoAsync(
            int messageId,
            CancellationToken ct)
        {
            GetMessageReplyInfoVO vo = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                where m.MessageId == messageId

                select new GetMessageReplyInfoVO(
                    p1.Id,
                    p1.ElectronicSubjectName,
                    t.ResponseTemplateId,
                    m.Subject,
                    m.Rnu)
                ).SingleAsync(ct);

            return vo;
        }
    }
}
