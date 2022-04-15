namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetSummaryAsSenderVO(
           string FileName,
           byte[] Summary,
           string ContentType);
    }
}
