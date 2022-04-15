namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetPassiveProfileDataVO(
            string FirstName,
            string MiddleName,
            string LastName,
            string? Address,
            string Email,
            string Phone);
    }
}
