namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetMessageBlobsVO(
            int MessageBlobId,
            int ProfileKeyId,
            string Provider,
            string KeyName,
            string OaepPadding,
            byte[] EncryptedKey);
    }
}
