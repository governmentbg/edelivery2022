using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TemplateProfile
    {
        // EF constructor
        private TemplateProfile()
        {
        }

        public TemplateProfile(
            int profileId,
            bool canSend,
            bool canReceive)
        {
            this.ProfileId = profileId;
            this.CanSend = canSend;
            this.CanReceive = canReceive;
        }

        public int TemplateId { get; set; }

        public int ProfileId { get; set; }

        public bool CanSend { get; set; }

        public bool CanReceive { get; set; }
    }
}

namespace ED.Domain
{
    class TemplateProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TemplateProfiles";

            var builder = modelBuilder.Entity<TemplateProfile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.TemplateId, e.ProfileId });
        }
    }
}
