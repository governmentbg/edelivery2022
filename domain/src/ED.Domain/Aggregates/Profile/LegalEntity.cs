using System;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CA1724 // Type names should not match namespaces

namespace ED.Domain
{
    public class LegalEntity
    {
        // EF constructor
        private LegalEntity()
        {
            this.Name = null!;
        }

        public LegalEntity(string name)
        {
            this.Name = name;
        }

        public Guid LegalEntityId { get; set; }

        public string Name { get; set; }

        public Guid? ParentLegalEntityId { get; set; }

        public void Update(string name)
        {
            this.Name = name;
        }
    }

    class LegalEntityMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "LegalEntities";

            var builder = modelBuilder.Entity<LegalEntity>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.LegalEntityId);
            builder.Property(e => e.LegalEntityId).ValueGeneratedNever();
        }
    }
}
