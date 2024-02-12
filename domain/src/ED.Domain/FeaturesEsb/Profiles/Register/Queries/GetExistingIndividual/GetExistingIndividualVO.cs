namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        public record GetExistingIndividualVO(
            int ProfileId,
            ProfileType ProfileType,
            bool IsPassive,
            bool IsRegistered);
    }
}
