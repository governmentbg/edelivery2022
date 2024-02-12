using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class SmsDelivery
    {
        // EF constructor
        private SmsDelivery()
        {
        }

        public SmsDelivery(
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

        public long SmsDeliveryId { get; private set; }

        public DateTime SentDate { get; private set; }

        public int MsgId { get; private set; }

        public DeliveryStatus Status { get; set; }

        public bool Charge { get; private set; }

        public string? Tag { get; private set; }
    }

    class SmsDeliveryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "reports";
            var tableName = "SmsDelivery";

            var builder = modelBuilder.Entity<SmsDelivery>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.SentDate, e.SmsDeliveryId });

            builder.Property(e => e.SmsDeliveryId).ValueGeneratedOnAdd();

            builder.Property(e => e.Status).HasColumnType("TINYINT");
        }
    }
}

