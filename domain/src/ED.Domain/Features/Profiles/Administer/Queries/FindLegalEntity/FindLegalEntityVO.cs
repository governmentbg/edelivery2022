namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record FindLegalEntityVO(
            int ProfileId,
            string Name);
    }
}
