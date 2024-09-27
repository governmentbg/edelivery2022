using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TicketStat
    {
        // EF constructor
        private TicketStat()
        {
        }

        public DateTime TicketStatDate { get; set; }

        public int ReceivedTicketIndividuals { get; set; }
        public int ReceivedPenalDecreeIndividuals { get; set; }
        public int ReceivedTicketLegalEntites { get; set; }
        public int ReceivedPenalDecreeLegalEntites { get; set; }

        public int InternalServed { get; set; }
        public int ExternalServed { get; set; }
        public int Annulled { get; set; }

        public int EmailNotifications { get; set; }
        public int PhoneNotifications { get; set; }

        public int DeliveredTicketIndividuals { get; set; }
        public int DeliveredPenalDecreeIndividuals { get; set; }
        public int DeliveredTicketLegalEntites { get; set; }
        public int DeliveredPenalDecreeLegalEntites { get; set; }

        public int SentToActiveProfiles { get; set; }
        public int SentToPassiveProfiles { get; set; }
    }

    class TicketStatMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "reports";
            var tableName = "TicketStats";

            var builder = modelBuilder.Entity<TicketStat>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.TicketStatDate });
        }
    }
}
