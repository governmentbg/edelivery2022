namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetMessageAccessKeyVO(
            int ProfileKeyId,
            byte[] EncryptedKey);
    }
}
