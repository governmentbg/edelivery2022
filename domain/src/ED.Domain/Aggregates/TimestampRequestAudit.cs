using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TimestampRequestAudit
    {
        // EF constructor
        private TimestampRequestAudit()
        {
        }

        public int Id { get; set; }

        public int? MessageId { get; set; }

        public int? BlobId { get; set; }

        public DateTime DateSent { get; set; }

        public TimestampRequestAuditStatus Status { get; set; }

        public string? Description { get; set; }
    }

    class TimestampRequestAuditMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TimeStampRequestsAuditLog";

            var builder = modelBuilder.Entity<TimestampRequestAudit>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.DateSent, e.Status, e.Id });
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
