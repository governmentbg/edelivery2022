namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record FindIndividualVO(
            int ProfileId,
            string Name);
    }
}
