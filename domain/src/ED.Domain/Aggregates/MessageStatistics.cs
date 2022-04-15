using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessageStatistics
    {
        // EF constructor
        private MessageStatistics()
        {
            this.Month = null!;
        }

        public MessageStatistics(
            DateTime monthDate,
            int messagesSent,
            int messagesReceived)
        {
            this.MonthDate = monthDate;
            this.Month = monthDate.ToString("yyyy-MM");
            this.MessagesSent = messagesSent;
            this.MessagesReceived = messagesReceived;
        }

        public int Id { get; set; }

        public DateTime MonthDate { get; set; }

        public string Month { get; set; }

        public int MessagesSent { get; set; }

        public int MessagesReceived { get; set; }
    }

    class MessageStatisticsMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "StatisticsMessagesByMonth";

            var builder = modelBuilder.Entity<MessageStatistics>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
