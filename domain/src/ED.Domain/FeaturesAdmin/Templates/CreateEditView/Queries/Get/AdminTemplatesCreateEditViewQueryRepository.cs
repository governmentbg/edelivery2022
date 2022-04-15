using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTemplatesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesCreateEditViewQueryRepository : IAdminTemplatesCreateEditViewQueryRepository
    {
        public async Task<GetVO> GetAsync(int templateId, CancellationToken ct)
        {
            GetVO vo = await (
                from t in this.DbContext.Set<Template>()

                join rlsl in this.DbContext.Set<LoginSecurityLevel>()
                    on t.ReadLoginSecurityLevelId equals rlsl.LoginSecurityLevelId

                join wlsl in this.DbContext.Set<LoginSecurityLevel>()
                    on t.WriteLoginSecurityLevelId equals wlsl.LoginSecurityLevelId

                join rt in this.DbContext.Set<Template>()
                    on t.ResponseTemplateId equals rt.TemplateId
                    into j1
                from rt in j1.DefaultIfEmpty()

                join b in this.DbContext.Set<Blob>()
                    on t.BlobId equals b.BlobId
                    into j2
                from b in j2.DefaultIfEmpty()

                where t.TemplateId == templateId

                select new GetVO(
                    t.TemplateId,
                    t.Name,
                    t.IdentityNumber,
                    t.Content,
                    t.ResponseTemplateId,
                    rt.Name,
                    t.CreateDate,
                    t.CreatedByAdminUserId,
                    t.PublishDate,
                    t.PublishedByAdminUserId,
                    t.ArchiveDate,
                    t.ArchivedByAdminUserId,
                    t.IsSystemTemplate,
                    t.ReadLoginSecurityLevelId,
                    rlsl.Name,
                    t.WriteLoginSecurityLevelId,
                    wlsl.Name,
                    t.BlobId,
                    b.FileName,
                    t.SenderDocumentField,
                    t.RecipientDocumentField,
                    t.SubjectDocumentField,
                    t.DateSentDocumentField,
                    t.DateReceivedDocumentField,
                    t.SenderSignatureDocumentField,
                    t.RecipientSignatureDocumentField))
                .FirstAsync(ct);

            return vo;
        }
    }
}
