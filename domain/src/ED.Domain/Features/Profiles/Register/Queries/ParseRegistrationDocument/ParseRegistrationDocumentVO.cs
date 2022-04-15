namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        public record ParseRegistrationDocumentVO(
            bool IsSuccessful,
            ParseRegistrationDocumentVOResult? Result);

        public record ParseRegistrationDocumentVOResult(
            string Name,
            string Identifier,
            string Phone,
            string Email,
            string Residence,
            string? City,
            string? State,
            string? Country);
    }
}
