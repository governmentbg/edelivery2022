using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ViberDelivery
    {
        // EF constructor
        private ViberDelivery()
        {
        }

        public ViberDelivery(
            DeliveryStatus status,
            int msgId,
            bool charge,
            string? tag)
        {
            this.SentDate = DateTime.Now;
            this.Status = status;
            this.MsgId = msgId;
            this.Charge = charge;
            this.Tag = tag;
        }

        public long ViberDeliveryId { get; set; }

        public DateTime SentDate { get; set; }

        public int MsgId { get; set; }

        public DeliveryStatus Status { get; set; }

        public bool Charge { get; set; }

        public string? Tag { get; set; }
    }

    class ViberDeliveryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "reports";
            var tableName = "ViberDelivery";

            var builder = modelBuilder.Entity<ViberDelivery>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.SentDate, e.ViberDeliveryId });

            builder.Property(e => e.ViberDeliveryId).ValueGeneratedOnAdd();

            builder.Property(e => e.Status).HasColumnType("TINYINT");
        }
    }
}


