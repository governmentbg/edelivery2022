using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageBlobAccessKey
    {
        // EF constructor
        private MessageBlobAccessKey()
        {
            this.EncryptedKey = null!;
        }

        public MessageBlobAccessKey(
            int profileId,
            int profileKeyId,
            byte[] encryptedKey)
        {
            this.ProfileId = profileId;
            this.ProfileKeyId = profileKeyId;
            this.EncryptedKey = encryptedKey;
        }

        public int ProfileId { get; set; }

        public int MessageBlobId { get; set; }

        public int ProfileKeyId { get; set; }

        public byte[] EncryptedKey { get; set; }
    }

    class MessageBlobAccessKeyMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageBlobAccessKeys";

            var builder = modelBuilder.Entity<MessageBlobAccessKey>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageBlobId, e.ProfileId });
        }
    }
}
