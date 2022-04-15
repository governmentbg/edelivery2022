namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetBlobVO(
            int BlobId,
            int? CreatedByAdminUserId,
            byte[] EncryptedKey,
            string SystemProfileKeyProvider,
            string SystemProfileKeyKeyName,
            string SystemProfileKeyOaepPadding);
    }
}
