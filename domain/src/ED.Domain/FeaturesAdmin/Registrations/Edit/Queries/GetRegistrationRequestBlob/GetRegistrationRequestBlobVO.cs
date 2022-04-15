namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        public record GetRegistrationRequestBlobVO(
            int BlobId,
            int CreatedByLoginId,
            byte[] EncryptedKey,
            string SystemProfileKeyProvider,
            string SystemProfileKeyKeyName,
            string SystemProfileKeyOaepPadding);
    }
}
