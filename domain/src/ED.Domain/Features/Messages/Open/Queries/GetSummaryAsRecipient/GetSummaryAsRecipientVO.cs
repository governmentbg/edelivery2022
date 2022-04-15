namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetSummaryAsRecipientVO(
           string FileName,
           byte[] Summary,
           string ContentType);
    }
}
