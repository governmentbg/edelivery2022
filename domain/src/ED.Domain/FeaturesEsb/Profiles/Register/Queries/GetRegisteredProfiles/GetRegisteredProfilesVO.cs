namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        public record GetRegisteredProfilesVO(
            int ProfileId,
            string Identifier,
            string Name,
            string Email,
            string Phone,
            GetRegisteredProfilesVOAddress? Address,
            bool IsActivated,
            bool IsPassive,
            int TargetGroupId,
            string TargetGroupName);

        public record GetRegisteredProfilesVOAddress(
            int AddressId,
            string? Residence,
            string? City,
            string? State,
            string? CountryIso);
    }
}
