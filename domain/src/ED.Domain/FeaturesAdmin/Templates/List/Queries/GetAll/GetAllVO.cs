using System;

namespace ED.Domain
{
    public partial interface IAdminTemplatesListQueryRepository
    {
        public record GetAllVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            string? Category,
            DateTime CreateDate,
            DateTime? PublishDate,
            DateTime? ArchiveDate,
            bool IsSystemTemplate,
            string ReadLoginSecurityLevelName,
            string WriteLoginSecurityLevelName);
    }
}
