using System;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record GetVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            string Content,
            int? ResponseTemplateId,
            string? ResponseTemplateName,
            DateTime CreateDate,
            int CreatedBy,
            DateTime? PublishDate,
            int? PublishedBy,
            DateTime? ArchiveDate,
            int? ArchivedBy,
            bool IsSystemTemplate,
            int ReadLoginSecurityLevelId,
            string ReadLoginSecurityLevelName,
            int WriteLoginSecurityLevelId,
            string WriteLoginSecurityLevelName,
            int? BlobId,
            string? BlobName,
            string? SenderDocumentField,
            string? RecipientDocumentField,
            string? SubjectDocumentField,
            string? DateSentDocumentField,
            string? DateReceivedDocumentField,
            string? SenderSignatureDocumentField,
            string? RecipientSignatureDocumentField);
    }
}
