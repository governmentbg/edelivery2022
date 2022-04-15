using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageBlob
    {
        public record MessageBlobAccessKeyDO(
            int ProfileId,
            int ProfileKeyId,
            byte[] EncryptedKey);

        // EF constructor
        public MessageBlob()
        {
        }

        public MessageBlob(
            int blobId,
            MessageBlobAccessKeyDO[] keys)
        {
            this.BlobId = blobId;

            this.messageBlobAccessKeys.AddRange(
                keys
                    .Select(e => new MessageBlobAccessKey(
                        e.ProfileId,
                        e.ProfileKeyId,
                        e.EncryptedKey)));
        }

        public int MessageBlobId { get; set; }

        public int MessageId { get; set; }

        public int BlobId { get; set; }

        private List<MessageBlobAccessKey> messageBlobAccessKeys = new();

        public IReadOnlyCollection<MessageBlobAccessKey> MessageBlobAccessKeys =>
            this.messageBlobAccessKeys.AsReadOnly();

        public void AddRecipients(MessageBlobAccessKeyDO[] keys)
        {
            this.messageBlobAccessKeys.AddRange(
                keys
                    .Select(e => new MessageBlobAccessKey(
                        e.ProfileId,
                        e.ProfileKeyId,
                        e.EncryptedKey)));
        }
    }

    class MessageBlobMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageBlobs";

            var builder = modelBuilder.Entity<MessageBlob>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.MessageBlobId);
            builder.Property(e => e.MessageBlobId).ValueGeneratedOnAdd();

            builder.HasMany(e => e.MessageBlobAccessKeys)
                .WithOne()
                .HasForeignKey(e => e.MessageBlobId);
        }
    }
}
