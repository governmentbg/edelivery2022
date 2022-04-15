using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class MessagesAccessCode
    {
        // EF constructor
        private MessagesAccessCode()
        {
            this.ReceiverFirstName = null!;
            this.ReceiverMiddleName = null!;
            this.ReceiverLastName = null!;
            this.ReceiverPhone = null!;
            this.ReceiverEmail = null!;
        }

        public MessagesAccessCode(
            Guid accessCode,
            string firstName,
            string middleName,
            string lastName,
            string phone,
            string email)
        {
            this.AccessCode = accessCode;
            this.ReceiverFirstName = firstName;
            this.ReceiverMiddleName = middleName;
            this.ReceiverLastName = lastName;
            this.ReceiverPhone = phone;
            this.ReceiverEmail = email;
        }

        public int MessageId { get; set; }

        public Guid AccessCode { get; set; }

        public string ReceiverFirstName { get; set; }

        public string ReceiverMiddleName { get; set; }

        public string ReceiverLastName { get; set; }

        public string ReceiverPhone { get; set; }

        public string ReceiverEmail { get; set; }
    }

    class MessagesAccessCodeMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "MessagesAccessCodes";

            var builder = modelBuilder.Entity<MessagesAccessCode>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.MessageId);
        }
    }
}
