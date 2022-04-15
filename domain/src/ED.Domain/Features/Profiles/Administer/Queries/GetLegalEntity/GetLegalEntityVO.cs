namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetLegalEntityVO(
            string Name,
            string Identifier,
            string Email,
            string Phone,
            string? Residence,
            string? ParentGuid,
            string? ParentName);
    }
}
