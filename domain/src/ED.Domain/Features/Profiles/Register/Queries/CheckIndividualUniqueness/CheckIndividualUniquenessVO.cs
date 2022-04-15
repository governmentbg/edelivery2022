namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        public record CheckIndividualUniquenessVO(
            bool IsUniqueIdentifier,
            bool IsUniqueEmail);
    }
}
