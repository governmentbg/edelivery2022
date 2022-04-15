namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileDataVO(
            GetProfileDataVOIndividualData? IndividualData,
            GetProfileDataVOLegalEntityData? LegalEntityData,
            string Identifier,
            string Phone,
            string EmailAddress,
            string? AddressCountryCode,
            string? AddressState,
            string? AddressCity,
            string? AddressResidence,
            int TargetGroupId,
            bool? EnableMessagesWithCode,
            bool IsActivated);

        public record GetProfileDataVOIndividualData(
            string FirstName,
            string MiddleName,
            string LastName);

        public record GetProfileDataVOLegalEntityData(
            string Name);
    }
}
