namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetSummaryVO(
           string FileName,
           byte[] Summary,
           string ContentType);
    }
}
