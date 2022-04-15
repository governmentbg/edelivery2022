namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetBlobTimestampVO(
            string FileName,
            byte[]? Timestamp);
    }
}
