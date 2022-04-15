using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Country
    {
        // EF constructor
        private Country()
        {
            this.CountryISO2 = null!;
            this.Name = null!;
        }

        public string CountryISO2 { get; set; }

        public string Name { get; set; }
    }

    class CountryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Countries";

            var builder = modelBuilder.Entity<Country>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.CountryISO2);
            builder.Property(e => e.CountryISO2).ValueGeneratedNever();
        }
    }
}
