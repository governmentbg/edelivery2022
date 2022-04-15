using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class RecipientGroupProfile
    {
        // EF constructor
        private RecipientGroupProfile()
        {
        }

        public RecipientGroupProfile(int profileId)
        {
            this.ProfileId = profileId;
        }

        public int RecipientGroupId { get; set; }

        public int ProfileId { get; set; }
    }

    class RecipientGroupProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "RecipientGroupProfiles";

            var builder = modelBuilder.Entity<RecipientGroupProfile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.RecipientGroupId, e.ProfileId });

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Profile))
                .WithMany()
                .HasForeignKey(nameof(RecipientGroupProfile.ProfileId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
