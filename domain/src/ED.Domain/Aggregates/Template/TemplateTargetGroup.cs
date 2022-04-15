using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TemplateTargetGroup
    {
        // EF constructor
        private TemplateTargetGroup()
        {
        }

        public TemplateTargetGroup(
            int targetGroupId,
            bool canSend,
            bool canReceive)
        {
            this.TargetGroupId = targetGroupId;
            this.CanSend = canSend;
            this.CanReceive = canReceive;
        }

        public int TemplateId { get; set; }

        public int TargetGroupId { get; set; }

        public bool CanSend { get; set; }

        public bool CanReceive { get; set; }
    }
}

namespace ED.Domain
{
    class TemplateTargetGroupMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TemplateTargetGroups";

            var builder = modelBuilder.Entity<TemplateTargetGroup>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.TemplateId, e.TargetGroupId });
        }
    }
}
