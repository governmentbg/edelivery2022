namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetTimestampNrdVO(
           string FileName,
           byte[]? Timestamp);
    }
}
