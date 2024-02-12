using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TicketStatus
    {
        // EF constructor
        private TicketStatus()
        {
        }

        public TicketStatus(int createdByLoginId)
        {
            this.Status = TicketStatusStatus.NonServed;
            this.CreateDate = DateTime.Now;
            this.CreatedByLoginId = createdByLoginId;
        }

        public static TicketStatus CreateInstanceForInternallyServed(
            DateTime openDate,
            int createdByLoginId)
            => new()
            {
                Status = TicketStatusStatus.InternallyServed,
                CreateDate = DateTime.Now,
                CreatedByLoginId = createdByLoginId,
            };

        public static TicketStatus CreateInstanceForExternallyServed(
            DateTime serveDate,
            int createdByLoginId)
            => new()
            {
                Status = TicketStatusStatus.ExternallyServed,
                ServeDate = serveDate,
                CreateDate = DateTime.Now,
                CreatedByLoginId = createdByLoginId,
            };

        public static TicketStatus CreateInstanceForAnnulled(
            DateTime annulDate,
            string annulmentReason,
            int createdByLoginId)
            => new()
            {
                Status = TicketStatusStatus.Annulled,
                AnnulDate = annulDate,
                AnnulmentReason = annulmentReason,
                CreateDate = DateTime.Now,
                CreatedByLoginId = createdByLoginId,
            };

        public int MessageId { get; set; }

        public int TicketStatusId { get; set; }

        public TicketStatusStatus Status { get; set; }

        // begin specific props
        public DateTime? ServeDate { get; set; }

        public DateTime? AnnulDate { get; set; }

        public string? AnnulmentReason { get; set; }
        // end specific props

        public DateTime CreateDate { get; set; }

        public int CreatedByLoginId { get; set; }

        public DateTime? SeenDate { get; set; }

        public int? SeenByLoginId { get; set; }

        public void MarkAsSeen(DateTime seenDate, int actionLoginId)
        {
            this.SeenDate = seenDate;
            this.SeenByLoginId = actionLoginId;
        }
    }

    class TicketStatusMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TicketStatuses";

            var builder = modelBuilder.Entity<TicketStatus>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.MessageId, e.TicketStatusId });
            builder.Property(e => e.TicketStatusId).ValueGeneratedOnAdd();

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(TicketStatus.CreatedByLoginId))
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(TicketStatus.SeenByLoginId))
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
