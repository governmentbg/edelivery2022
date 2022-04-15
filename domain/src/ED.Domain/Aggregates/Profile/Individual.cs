using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Individual
    {
        // EF constructor
        private Individual()
        {
            this.FirstName = null!;
            this.MiddleName = null!;
            this.LastName = null!;
        }

        public Individual(
            string firstName,
            string middleName,
            string lastName)
        {
            this.FirstName = firstName;
            this.MiddleName = middleName;
            this.LastName = lastName;
        }

        public Guid IndividualId { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public void Update(string firstName, string middleName, string lastName)
        {
            this.FirstName = firstName;
            this.MiddleName = middleName;
            this.LastName = lastName;
        }
    }

    class IndividualMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Individuals";

            var builder = modelBuilder.Entity<Individual>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.IndividualId);
            builder.Property(e => e.IndividualId).ValueGeneratedNever();
        }
    }
}
