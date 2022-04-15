using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetLoginInfoVO(
            Guid ElectronicSubjectId,
            string ElectronicSubjectName,
            string? Email,
            string? Phone,
            bool IsProfileLogin);
    }
}
