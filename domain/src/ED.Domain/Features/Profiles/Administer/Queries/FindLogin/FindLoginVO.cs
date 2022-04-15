namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record FindLoginVO(
            int LoginId,
            string LoginName,
            string ProfileIdentifier);
    }
}
