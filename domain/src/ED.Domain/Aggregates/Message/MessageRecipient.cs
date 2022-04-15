using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageRecipient
    {
        // EF constructor
        private MessageRecipient()
        {
        }

        public MessageRecipient(int profileId)
        {
            this.ProfileId = profileId;
        }

        public int MessageId { get; set; }

        public int ProfileId { get; set; }

        public int? LoginId { get; set; }

        public DateTime? DateReceived { get; set; }

        public byte[]? Timestamp { get; set; }

        public int? MessagePdfBlobId { get; set; }

        public byte[]? MessageSummary { get; set; }

        public string? MessageSummaryXml { get; set; }

        public void UpdateMessagePdfBlob(int? blobId)
        {
            this.MessagePdfBlobId = blobId;
        }
    }

    class MessageRecipientMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageRecipients";

            var builder = modelBuilder.Entity<MessageRecipient>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageId, e.ProfileId });

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(MessageRecipient.MessagePdfBlobId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
