namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetMessageAccessKeyVO(
            int ProfileKeyId,
            byte[] EncryptedKey);
    }
}
