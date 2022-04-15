using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    public class QueueMessage
    {
        // EF constructor
        private QueueMessage()
        {
            this.Payload = null!;
            this.Version = null!;
        }

        public QueueMessage(
            QueueMessageType type,
            string payload,
            DateTime? dueDate = null,
            string? tag = null,
            string? key = null)
        {
            // queue message always use UTC to prevent problems with daylight saving time
            var utcNow = DateTime.UtcNow;

            this.Status = QueueMessageStatus.Pending;
            this.Type = type;
            this.DueDate = (dueDate?.ToUniversalTime() ?? utcNow).Ticks;
            this.Key = key;
            this.Tag = tag;
            this.Payload = payload;
            this.FailedAttempts = 0;
            this.CreateDate = utcNow;
            this.StatusDate = utcNow;
            this.Version = null!;
        }

        public QueueMessageStatus Status { get; private set; }

        public QueueMessageType Type { get; private set; }

        public long DueDate { get; private set; }

        public int QueueMessageId { get; private set; }

        public string? Key { get; private set; }

        public string? Tag { get; private set; }

        public string Payload { get; private set; }

        public int FailedAttempts { get; private set; }

        public string? FailedAttemptsErrors { get; private set; }

        public DateTime CreateDate { get; private set; }

        public DateTime StatusDate { get; private set; }

        public byte[] Version { get; private set; }
    }

    class QueueMessageMapping : EntityMapping
    {
        private int hiLoBlockSize;

        public QueueMessageMapping(IOptions<DomainOptions> options)
        {
            this.hiLoBlockSize = options.Value.HiLoBlockSize;
        }

        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "QueueMessages";
            var sequenceName = $"QueueMessagesIdSequence";

            modelBuilder.HasSequence<int>(sequenceName, schema);

            var builder = modelBuilder.Entity<QueueMessage>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.Status, e.Type, e.DueDate, e.QueueMessageId });

            builder.Property(e => e.QueueMessageId)
                .ForSqlServerUseSpGetRangeSequenceHiLo(
                    sequenceName,
                    schema,
                    this.hiLoBlockSize);
            builder.Property(e => e.Version)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}
