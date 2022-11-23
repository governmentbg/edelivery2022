using System;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record GetVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            string? Category,
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
            string WriteLoginSecurityLevelName);
    }
}
