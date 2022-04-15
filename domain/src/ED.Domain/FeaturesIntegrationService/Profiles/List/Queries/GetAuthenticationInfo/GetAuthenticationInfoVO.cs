namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record GetAuthenticationInfoVO(
            int ProfileId,
            int LoginId,
            int? OperatorLoginId);
    }
}
