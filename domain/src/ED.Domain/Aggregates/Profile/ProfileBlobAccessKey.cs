using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfileBlobAccessKey
    {
        // EF constructor
        private ProfileBlobAccessKey()
        {
            this.EncryptedKey = null!;
        }

        public ProfileBlobAccessKey(
            int blobId,
            int profileKeyId,
            int? createdByLoginId,
            int? createdByAdminUserId,
            byte[] encryptedKey,
            ProfileBlobAccessKeyType type)
        {
            this.BlobId = blobId;
            this.ProfileKeyId = profileKeyId;
            this.CreatedByLoginId = createdByLoginId;
            this.CreatedByAdminUserId = createdByAdminUserId;
            this.EncryptedKey = encryptedKey;
            this.Type = type;
        }

        public int ProfileId { get; set; }

        public int BlobId { get; set; }

        public int ProfileKeyId { get; set; }

        public int? CreatedByLoginId { get; set; }

        public int? CreatedByAdminUserId { get; set; }

        public byte[] EncryptedKey { get; set; }

        public ProfileBlobAccessKeyType Type { get; set; }

        public string? Description { get; set; }
    }

    class ProfileBlobAccessKeyMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ProfileBlobAccessKeys";

            var builder = modelBuilder.Entity<ProfileBlobAccessKey>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.ProfileId, e.BlobId });

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(ProfileBlobAccessKey.BlobId))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(ProfileBlobAccessKey.CreatedByLoginId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
