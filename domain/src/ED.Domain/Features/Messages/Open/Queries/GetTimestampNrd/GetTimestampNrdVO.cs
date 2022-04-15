namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetTimestampNrdVO(
           string FileName,
           byte[]? Timestamp);
    }
}
