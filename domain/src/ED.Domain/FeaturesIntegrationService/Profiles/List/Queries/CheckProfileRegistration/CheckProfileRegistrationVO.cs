namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record CheckProfileRegistrationVO(
            bool HasRegistration,
            string ProfileIdentifier,
            string? ProfileSubjectId,
            string? ProfileName,
            bool? ProfileIsActivated,
            string? ProfileEmail,
            string? ProfilePhone,
            int? TargetGroupId)
        {
            public static CheckProfileRegistrationVO Empty(string identifier) => 
                new(false, identifier, null, null, null, null, null, null);
        }
    }
}
