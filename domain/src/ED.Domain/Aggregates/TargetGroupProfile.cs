using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TargetGroupProfile
    {
        // EF constructor
        private TargetGroupProfile()
        {
        }

        public TargetGroupProfile(int targetGroupId, int profileId)
        {
            this.TargetGroupId = targetGroupId;
            this.ProfileId = profileId;
        }

        public int TargetGroupId { get; set; }

        public int ProfileId { get; set; }
    }

    class TargetGroupProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TargetGroupProfiles";

            var builder = modelBuilder.Entity<TargetGroupProfile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.TargetGroupId, e.ProfileId });

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Profile))
                .WithMany()
                .HasForeignKey(nameof(TargetGroupProfile.ProfileId))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(typeof(TargetGroup))
                .WithMany()
                .HasForeignKey(nameof(TargetGroupProfile.TargetGroupId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
