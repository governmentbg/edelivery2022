namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetIndividualVO(
            string FirstName,
            string MiddleName,
            string LastName,
            string Identifier,
            string Email,
            string Phone,
            string? Residence);
    }
}
