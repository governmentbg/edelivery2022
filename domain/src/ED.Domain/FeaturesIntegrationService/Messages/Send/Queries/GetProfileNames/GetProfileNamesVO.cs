namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}
