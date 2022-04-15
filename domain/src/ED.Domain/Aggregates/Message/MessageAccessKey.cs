using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageAccessKey
    {
        // EF constructor
        private MessageAccessKey()
        {
            this.EncryptedKey = null!;
        }

        public MessageAccessKey(
            int profileId,
            int profileKeyId,
            byte[] encryptedKey)
        {
            this.ProfileId = profileId;
            this.ProfileKeyId = profileKeyId;
            this.EncryptedKey = encryptedKey;
        }

        public int ProfileId { get; set; }

        public int MessageId { get; set; }

        public int ProfileKeyId { get; set; }

        public byte[] EncryptedKey { get; set; }
    }

    class MessageAccessKeyMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageAccessKeys";

            var builder = modelBuilder.Entity<MessageAccessKey>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageId, e.ProfileId });
        }
    }
}
