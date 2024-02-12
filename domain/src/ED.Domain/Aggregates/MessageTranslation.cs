using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageTranslation
    {
        // EF constructor
        private MessageTranslation()
        {
            this.SourceLanguage = null!;
            this.TargetLanguage = null!;
        }

        public MessageTranslation(
            int messageId,
            int profileId,
            string sourceLanguage,
            string targetLanguage,
            int loginId,
            int?[] sourceBlobIds)
        {
            DateTime now = DateTime.Now;

            this.MessageId = messageId;
            this.ProfileId = profileId;
            this.SourceLanguage = sourceLanguage;
            this.TargetLanguage = targetLanguage;
            this.CreateDate = now;
            this.CreatedBy = loginId;
            this.ModifyDate = now;

            foreach (int? sourceBlobId in sourceBlobIds)
            {
                MessageTranslationRequest request = new(sourceBlobId);
                this.requests.Add(request);
            }
        }

        public int MessageId { get; set; }

        public int ProfileId { get; set; }

        public string SourceLanguage { get; set; }

        public string TargetLanguage { get; set; }

        public int MessageTranslationId { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime ModifyDate { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int? ArchivedBy { get; set; }

        private List<MessageTranslationRequest> requests = new();

        public IReadOnlyCollection<MessageTranslationRequest> Requests =>
            this.requests.AsReadOnly();

        public void UpdateRequest(
            int? sourceBlobId,
            long? requestId,
            MessageTranslationRequestStatus status,
            string? errorMessage)
        {
            MessageTranslationRequest request =
                this.requests.First(e => e.SourceBlobId == sourceBlobId);

            request.Update(requestId, status, errorMessage);

            this.ModifyDate = DateTime.Now;
        }

        public void CloseRequests()
        {
            foreach (MessageTranslationRequest request in this.requests)
            {
                request.Close();
            }
        }

        public void Archive(int loginId)
        {
            DateTime now = DateTime.Now;

            this.ModifyDate = now;
            this.ArchiveDate = now;
            this.ArchivedBy = loginId;

            this.requests.Clear();
        }
    }

    class MessageTranslationMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessageTranslations";

            var builder = modelBuilder.Entity<MessageTranslation>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageTranslationId });
            builder.Property(e => e.MessageTranslationId).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Requests)
                .WithOne()
                .HasForeignKey(e => new { e.MessageTranslationId });

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(MessageTranslation.CreatedBy))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(MessageTranslation.ArchivedBy))
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
