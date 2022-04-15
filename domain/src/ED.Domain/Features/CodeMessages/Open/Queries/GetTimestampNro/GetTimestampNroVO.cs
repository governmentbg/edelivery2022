namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetTimestampNroVO(
           string FileName,
           byte[]? Timestamp);
    }
}
