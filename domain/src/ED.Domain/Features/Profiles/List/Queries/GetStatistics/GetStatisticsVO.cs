namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        public record GetStatisticsVO(
            int LegalEntitiesCount,
            int PublicAdministrationsCount,
            int SocialOrganizationsCount);
    }
}
