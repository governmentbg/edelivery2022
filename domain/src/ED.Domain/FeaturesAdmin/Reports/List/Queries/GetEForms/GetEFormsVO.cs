namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetEFormsVO(
            string MessageSubject,
            int Count);
    }
}
