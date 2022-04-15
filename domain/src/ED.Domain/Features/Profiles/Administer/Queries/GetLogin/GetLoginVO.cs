namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetLoginVO(int LoginId);
    }
}
