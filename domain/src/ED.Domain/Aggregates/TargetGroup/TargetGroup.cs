using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class TargetGroup
    {
        public const int IndividualTargetGroupId = 1;
        public const int LegalEntityTargetGroupId = 2;
        public const int PublicAdministrationTargetGroupId = 3;
        public const int SocialOrganizationTargetGroupId = 4;

        // EF constructor
        private TargetGroup()
        {
            this.Name = null!;
        }

        public TargetGroup(string name, int adminUserId)
        {
            DateTime now = DateTime.Now;

            this.Name = name;

            this.CreateDate = now;
            this.CreatedByAdminUserId = adminUserId;

            this.ModifyDate = now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public void Update(string name, int adminUserId)
        {
            this.Name = name;

            this.ModifyDate = DateTime.Now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public void Archive(int adminUserId)
        {
            if (this.TargetGroupId == IndividualTargetGroupId
                || this.TargetGroupId == LegalEntityTargetGroupId
                || this.TargetGroupId == PublicAdministrationTargetGroupId
                || this.TargetGroupId == SocialOrganizationTargetGroupId)
            {
                throw new DomainException("Cannot archive a system target group");
            }

            DateTime now = DateTime.Now;

            this.ModifyDate = now;
            this.ModifiedByAdminUserId = adminUserId;

            this.ArchiveDate = now;
            this.ArchivedByAdminUserId = adminUserId;
        }

        public void AddRecipientTargetGroups(
            int[] targetGroupIds,
            int adminUserId)
        {
            int[] newRecipientTargetGroups = targetGroupIds
                .Where(e => !this.matrices.Select(m => m.RecipientTargetGroupId).Contains(e))
                .ToArray();

            this.matrices.AddRange(
                newRecipientTargetGroups
                    .Select(e => new TargetGroupMatrix(e)));

            this.ModifyDate = DateTime.Now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public void RemoveRecipientTargetGroup(
            int targetGroupId,
            int adminUserId)
        {
            this.matrices
                .RemoveAll(e => e.RecipientTargetGroupId == targetGroupId);

            this.ModifyDate = DateTime.Now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public int TargetGroupId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedByAdminUserId { get; set; }

        public DateTime ModifyDate { get; set; }

        public int ModifiedByAdminUserId { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int? ArchivedByAdminUserId { get; set; }

        private List<TargetGroupMatrix> matrices = new();

        public IReadOnlyCollection<TargetGroupMatrix> Matrices =>
            this.matrices.AsReadOnly();
    }

    class TargetGroupMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "TargetGroups";

            var builder = modelBuilder.Entity<TargetGroup>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.TargetGroupId);
            builder.Property(e => e.TargetGroupId).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Matrices)
                .WithOne()
                .HasForeignKey(e => e.SenderTargetGroupId);
        }
    }
}
