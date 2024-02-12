namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        public record GetProfileVO(
            int ProfileId,
            string Identifier,
            string Name,
            string Email,
            string Phone,
            GetProfileVOAddress? Address);

        public record GetProfileVOAddress(
            int AddressId,
            string? Residence,
            string? City,
            string? State,
            string? CountryIso);
    }
}
