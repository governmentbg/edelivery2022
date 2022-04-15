namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public record GetExistingIndividualVO(
            int ProfileId,
            ProfileType ProfileType,
            bool IsPassive,
            bool IsRegistered);
    }
}
