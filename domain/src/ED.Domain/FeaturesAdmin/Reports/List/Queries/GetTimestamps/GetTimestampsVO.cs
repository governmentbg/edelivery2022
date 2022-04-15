namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetTimestampsVO(
            int CountSuccess,
            int CountError);
    }
}
