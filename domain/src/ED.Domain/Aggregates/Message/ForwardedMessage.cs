using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial class ForwardedMessage
    {
        // EF constructor
        public ForwardedMessage()
        {
            this.ForwardedMessageSubject = null!;
            this.ForwardedMessageSenderProfileName = null!;
        }

        public ForwardedMessage(
            int forwardedMessageId,
            string forwardedMessageSubject,
            int forwardedMessageSenderProfileId,
            string forwardedMessageSenderProfileName,
            Guid forwardedMessageSenderProfileSubjectId,
            string forwardedMessageSenderLoginName)
        {
            this.ForwardedMessageId = forwardedMessageId;
            this.ForwardedMessageSubject = forwardedMessageSubject;
            this.ForwardedMessageSenderProfileId = forwardedMessageSenderProfileId;
            this.ForwardedMessageSenderProfileName = forwardedMessageSenderProfileName;
            this.ForwardedMessageSenderProfileSubjectId = forwardedMessageSenderProfileSubjectId;
            this.ForwardedMessageSenderLoginName = forwardedMessageSenderLoginName;
        }

        public int MessageId { get; set; }
        public int ForwardedMessageId { get; set; }
        public string ForwardedMessageSubject { get; set; }
        public int ForwardedMessageSenderProfileId { get; set; }
        public string ForwardedMessageSenderProfileName { get; set; }
        public Guid? ForwardedMessageSenderProfileSubjectId { get; set; } // todo unnecessary?
        public string? ForwardedMessageSenderLoginName { get; set; }
    }

    class ForwardedMessageMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ForwardedMessages";

            var builder = modelBuilder.Entity<ForwardedMessage>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.MessageId);
        }
    }
}
