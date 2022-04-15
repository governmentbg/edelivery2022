namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetTimestampNroVO(
           string FileName,
           byte[]? Timestamp);
    }
}
