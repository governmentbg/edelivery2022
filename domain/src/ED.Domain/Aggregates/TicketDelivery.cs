using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TicketDelivery
    {
        // EF constructor
        private TicketDelivery()
        {
        }

        public TicketDelivery(
            int messageId,
            DeliveryStatus status)
        {
            this.SentDate = DateTime.Now;
            this.MessageId = messageId;
            this.Status = status;
        }

        public long TicketDeliveryId { get; set; }

        public int MessageId { get; set; }

        public DateTime SentDate { get; set; }

        public DeliveryStatus Status { get; set; }
    }

    class TicketDeliveryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "reports";
            var tableName = "TicketDelivery";

            var builder = modelBuilder.Entity<TicketDelivery>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.SentDate, e.TicketDeliveryId });

            builder.Property(e => e.TicketDeliveryId).ValueGeneratedOnAdd();

            builder.Property(e => e.Status).HasColumnType("TINYINT");
        }
    }
}
