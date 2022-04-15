namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        public record GetRegistrationSystemProfileBlobVO(
            int BlobId,
            byte[] EncryptedKey,
            string SystemProfileKeyProvider,
            string SystemProfileKeyKeyName,
            string SystemProfileKeyOaepPadding);
    }
}
