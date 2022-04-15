namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record CheckIndividualRegistrationVO(
            string ProfileIdentifier,
            bool HasRegistration,
            string? ProfileName,
            CheckIndividualRegistrationVOProfile[] Profiles);

        public record CheckIndividualRegistrationVOProfile(
            string ProfileName,
            string ProfileIdentifier,
            int TargetGroupId);
    }
}
