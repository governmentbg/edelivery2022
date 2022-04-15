namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record CheckLegalEntityRegistrationVO(
            string ProfileIdentifier,
            bool HasRegistration,
            string? ProfileName,
            string? ProfilePhone,
            string? ProfileEmail,
            CheckLegalEntityRegistrationVOLogin[] Logins);

        public record CheckLegalEntityRegistrationVOLogin(
            string LoginName,
            string ProfileIdentifier,
            int TargetGroupId);
    }
}
