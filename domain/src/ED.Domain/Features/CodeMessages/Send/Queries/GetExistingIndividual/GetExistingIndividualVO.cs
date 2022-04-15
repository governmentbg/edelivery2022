namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        public record GetExistingIndividualVO(
            int ProfileId,
            ProfileType ProfileType,
            bool IsPassive,
            bool IsRegistered);
    }
}
