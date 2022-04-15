namespace ED.Domain
{
    public partial interface IProfileSeosQueryRepository
    {
        public record GetProfileVO(
            string ProfileName,
            string Identifier,
            string Email,
            string Phone,
            string? Residence,
            string? City);
    }
}
