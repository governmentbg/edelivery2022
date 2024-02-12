using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageTranslationRequest
    {
        // EF constructor
        private MessageTranslationRequest()
        {
        }

        public MessageTranslationRequest(
            int? sourceBlobId)
        {
            this.SourceBlobId = sourceBlobId;
            this.Status = MessageTranslationRequestStatus.Pending;
        }

        public int MessageTranslationId { get; set; }

        public int MessageTranslationRequestId { get; set; }

        public long? RequestId { get; set; }

        public int? SourceBlobId { get; set; }

        public int? TargetBlobId { get; set; }

        public MessageTranslationRequestStatus Status { get; set; }

        public string? ErrorMessage { get; set; }

        public void Update(
            long? requestId,
            MessageTranslationRequestStatus status,
            string? errorMessage)
        {
            this.RequestId = requestId;
            this.Status = status;
            this.ErrorMessage = errorMessage;
        }

        public void Close()
        {
            if (this.Status == MessageTranslationRequestStatus.Pending
                || this.Status == MessageTranslationRequestStatus.Processing)
            {
                this.Status = MessageTranslationRequestStatus.Errored;
                this.ErrorMessage = "Автоматично затваряне на заявката за превод";
            }
        }
    }

    class MessageTranslationRequestMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageTranslationRequests";

            var builder = modelBuilder.Entity<MessageTranslationRequest>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageTranslationId, e.MessageTranslationRequestId });
            builder.Property(e => e.MessageTranslationRequestId).ValueGeneratedOnAdd();

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(MessageTranslationRequest.SourceBlobId))
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(MessageTranslationRequest.TargetBlobId))
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
