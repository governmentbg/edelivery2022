using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<GetTemplateVO> GetTemplateAsync(
            int templateId,
            CancellationToken ct)
        {
            GetTemplateVO vo = await (
                from t in this.DbContext.Set<Template>()

                where t.TemplateId == templateId

                select new GetTemplateVO(
                    t.BlobId!.Value,
                    t.Content,
                    t.SenderDocumentField,
                    t.RecipientDocumentField,
                    t.SubjectDocumentField,
                    t.DateSentDocumentField,
                    t.DateReceivedDocumentField,
                    t.SenderSignatureDocumentField,
                    t.RecipientSignatureDocumentField))
                .SingleAsync(ct);

            return vo;
        }
    }
}
