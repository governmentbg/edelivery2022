using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private IEnumerable<IEntityMapping> mappings;

        public DbContext(DbContextOptions<DbContext> options,
            IEnumerable<IEntityMapping> mappings)
            : base(options)
        {
            this.mappings = mappings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var mapping in this.mappings)
            {
                mapping.AddFluentMapping(modelBuilder);
            }

            this.RegisterQoTypes(
                modelBuilder,
                Assembly.GetAssembly(typeof(DomainModule))!);
        }

        private void RegisterQoTypes(ModelBuilder modelBuilder, Assembly assembly)
        {
            var qoTypes =
                assembly
                .GetTypes()
                .Where(type =>
                    type.GetCustomAttributes(typeof(KeylessAttribute), true)
                    .Length > 0);

            foreach (var qoType in qoTypes)
            {
                modelBuilder.Entity(qoType);
            }
        }
    }
}
