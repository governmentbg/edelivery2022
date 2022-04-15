namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        public record GetRegisteredIndividualVO(
            int ProfileId,
            string Guid,
            string Email,
            string Name,
            string Phone,
            string Identifier);
    }
}
