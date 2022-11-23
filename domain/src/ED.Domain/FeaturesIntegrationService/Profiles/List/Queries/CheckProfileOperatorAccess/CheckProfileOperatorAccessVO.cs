namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record CheckProfileOperatorAccessVO(
            bool HasAccess,
            int? OperatorLoginId);
    }
}
