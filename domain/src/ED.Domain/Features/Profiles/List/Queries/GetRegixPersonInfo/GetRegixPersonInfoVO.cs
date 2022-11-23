using System;

namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        public record GetRegixPersonInfoVO(
            string? ErrorMessage,
            bool Success,
            string FirstName,
            string SurName,
            string FamilyName,
            DateTime? BirthDate);
    }
}
