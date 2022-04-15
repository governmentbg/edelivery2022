namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetBlobTimestampVO(
            string FileName,
            byte[]? Timestamp);
    }
}
