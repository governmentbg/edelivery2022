namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        public record GetExistingIndividualVO(
            int ProfileId,
            ProfileType ProfileType,
            bool IsPassive,
            bool IsRegistered);
    }
}
