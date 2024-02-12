using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Ticket
    {
        // EF constructor
        private Ticket()
        {
            this.Type = null!;
        }

        public Ticket(
            int messageId,
            string type,
            DateTime violationDate,
            string? email,
            string? phone,
            int senderProfileId,
            string? documentSeries,
            string documentNumber,
            DateTime issueDate,
            string recipientIdentifier,
            string? documentIdentifier,
            int createdByLoginId)
        {
            this.MessageId = messageId;
            this.Type = type;
            this.ViolationDate = violationDate;
            this.Email = email;
            this.Phone = phone;
            this.SenderProfileId = senderProfileId;
            this.DocumentSeries = documentSeries;
            this.DocumentNumber = documentNumber;
            this.IssueDate = issueDate;
            this.RecipientIdentifier = recipientIdentifier;
            this.DocumentIdentifier = documentIdentifier;
            this.statuses.Add(new(createdByLoginId));
        }

        public int MessageId { get; set; }

        public string Type { get; set; }

        public DateTime ViolationDate { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public int? SenderProfileId { get; set; }

        public string? DocumentSeries { get; set; }

        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public string? RecipientIdentifier { get; set; }

        public string? DocumentIdentifier {  get; set; }

        private List<TicketStatus> statuses = new();

        public IReadOnlyCollection<TicketStatus> Statuses =>
            this.statuses.AsReadOnly();

        public void InternalServe(DateTime openDate, int createdByLoginId)
        {
            TicketStatus lastStatus =
                this.statuses.OrderByDescending(e => e.TicketStatusId).First();

            if (lastStatus.Status == TicketStatusStatus.NonServed)
            {
                TicketStatus internallyServedTicketStatus =
                    TicketStatus.CreateInstanceForInternallyServed(openDate, createdByLoginId);

                internallyServedTicketStatus.MarkAsSeen(openDate, createdByLoginId);

                this.statuses.Add(internallyServedTicketStatus);
            }
            else
            {
                lastStatus.MarkAsSeen(openDate, createdByLoginId);
            }
        }

        public void ExternalServe(DateTime serveDate, int createdByLoginId)
        {
            TicketStatus externallyServedTicketStatus =
                TicketStatus.CreateInstanceForExternallyServed(serveDate, createdByLoginId);

            this.statuses.Add(externallyServedTicketStatus);
        }

        public void Annul(
            DateTime annulDate,
            string annulmentReason,
            int createdByLoginId)
        {
            TicketStatus annulledTicketStatus =
                TicketStatus.CreateInstanceForAnnulled(
                    annulDate,
                    annulmentReason,
                    createdByLoginId);

            this.statuses.Add(annulledTicketStatus);
        }

        public void MarkStatusAsSeen(DateTime seenDate, int actionLoginId)
        {
            TicketStatus lastStatus =
                this.statuses.OrderByDescending(e => e.TicketStatusId).First();

            lastStatus.MarkAsSeen(seenDate, actionLoginId);
        }

        public bool IsStatusSeen()
        {
            TicketStatus lastStatus =
                this.statuses.OrderByDescending(e => e.TicketStatusId).First();

            return lastStatus.SeenDate.HasValue;
        }
    }

    class TicketMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Tickets";

            var builder = modelBuilder.Entity<Ticket>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.MessageId);

            builder.HasMany(e => e.Statuses)
                .WithOne()
                .HasForeignKey(e => e.MessageId);
        }
    }
}
