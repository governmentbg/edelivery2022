namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetTemporaryOrStorageBlobsVO(
            int ProfileKeyId,
            string Provider,
            string KeyName,
            string OaepPadding,
            int BlobId,
            string FileName,
            string Hash,
            string HashAlgorith,
            byte[] EncryptedKey);
    }
}
