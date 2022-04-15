using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Address
    {
        // EF constructor
        private Address()
        {
        }

        public Address(string residence)
        {
            this.Residence = residence;
        }

        public Address(
            string residence,
            string? city,
            string? state,
            string? country)
        {
            this.Residence = residence;
            this.City = city;
            this.State = state;
            this.Country = country;
        }

        public int AddressId { get; set; }

        public string? Country { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? Residence { get; set; }

        public void Update(string residence)
        {
            this.Residence = residence;
        }
    }

    class AddressMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Addresses";

            var builder = modelBuilder.Entity<Address>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.AddressId);
            builder.Property(e => e.AddressId).ValueGeneratedOnAdd();
        }
    }
}
