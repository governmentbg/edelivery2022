using System.Data.Entity;

namespace EDelivery.SEOS.DBEntities
{
    public class EDeliveryDbContext : DbContext
    {
        public EDeliveryDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<Profiles> Profiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
