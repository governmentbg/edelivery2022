using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class EmailDelivery
    {
        // EF constructor
        private EmailDelivery()
        {
        }

        public EmailDelivery(
            DeliveryStatus status,
            string? tag)
        {
            this.SentDate = DateTime.Now;
            this.Status = status;
            this.Tag = tag;
        }

        public long EmailDeliveryId { get; private set; }

        public DateTime SentDate { get; private set; }

        public DeliveryStatus Status { get; private set; }

        public string? Tag { get; private set; }
    }

    class EmailDeliveryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "reports";
            var tableName = "EmailDelivery";

            var builder = modelBuilder.Entity<EmailDelivery>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.SentDate, e.EmailDeliveryId });

            builder.Property(e => e.EmailDeliveryId).ValueGeneratedOnAdd();

            builder.Property(e => e.Status).HasColumnType("TINYINT");
        }
    }
}

