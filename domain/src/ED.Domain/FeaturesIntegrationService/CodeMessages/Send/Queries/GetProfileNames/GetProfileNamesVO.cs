namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}
