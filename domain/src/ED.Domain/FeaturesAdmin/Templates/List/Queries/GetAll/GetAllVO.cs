using System;

namespace ED.Domain
{
    public partial interface IAdminTemplatesListQueryRepository
    {
        public record GetAllVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            string Content,
            DateTime CreateDate,
            DateTime? PublishDate,
            DateTime? ArchiveDate,
            bool IsSystemTemplate,
            int ReadLoginSecurityLevelId,
            string ReadLoginSecurityLevelName,
            int WriteLoginSecurityLevelId,
            string WriteLoginSecurityLevelName);
    }
}
