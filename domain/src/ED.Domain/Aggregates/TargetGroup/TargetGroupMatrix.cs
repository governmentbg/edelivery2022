using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TargetGroupMatrix
    {
        // EF constructor
        private TargetGroupMatrix()
        {
        }

        public TargetGroupMatrix(int recipientTargetGroupId)
        {
            this.RecipientTargetGroupId = recipientTargetGroupId;
        }

        public int TargetGroupMatrixId { get; set; }

        public int SenderTargetGroupId { get; set; }

        public int RecipientTargetGroupId { get; set; }
    }

    class TargetGroupMatrixMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TargetGroupMatrix";

            var builder = modelBuilder.Entity<TargetGroupMatrix>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.TargetGroupMatrixId);
            builder.Property(e => e.TargetGroupMatrixId).ValueGeneratedOnAdd();

            builder.HasOne(typeof(TargetGroup))
                .WithMany()
                .HasForeignKey(nameof(TargetGroupMatrix.RecipientTargetGroupId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
