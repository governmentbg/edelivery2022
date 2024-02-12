using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CA1724 // Type names should not match namespaces

namespace ED.Domain
{
    public class Template
    {
        public const int SystemTemplateId = 1;
        public const int SystemForwardTemplateId = 2;
        public const int TicketTemplate = 10001;

        public static Guid SystemTemplateBodyFieldId() =>
            new("179ea4dc-7879-43ad-8073-72b263915656");
        public static Guid SystemForwardTemplateBodyFieldId() =>
            new("8d3747fe-dcee-4371-92f2-75549a3f804d");

        // EF constructor
        private Template()
        {
            this.Name = null!;
            this.Content = null!;
            this.IdentityNumber = null!;

            this.ResponseTemplate = null!;
            this.ReadLoginSecurityLevel = null!;
            this.WriteLoginSecurityLevel = null!;
        }

        public Template(
            string name,
            string identityNumber,
            string? category,
            string content,
            int? responseTemplateId,
            bool isSystemTemplate,
            int createdByAdminUserId,
            int readLoginSecurityLevelId,
            int writeLoginSecurityLevelId)
        {
            this.Name = name;
            this.IdentityNumber = identityNumber;
            this.Category = category;
            this.Content = content;
            this.ResponseTemplateId = responseTemplateId;
            this.IsSystemTemplate = isSystemTemplate;
            this.CreatedByAdminUserId = createdByAdminUserId;
            this.ReadLoginSecurityLevelId = readLoginSecurityLevelId;
            this.WriteLoginSecurityLevelId = writeLoginSecurityLevelId;

            this.CreateDate = DateTime.Now;
            this.ResponseTemplate = null!;
            this.ReadLoginSecurityLevel = null!;
            this.WriteLoginSecurityLevel = null!;
        }

        public int TemplateId { get; set; }

        public string Name { get; set; }

        public string IdentityNumber { get; set; }

        public string Content { get; set; }

        public int? ResponseTemplateId { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedByAdminUserId { get; set; }

        public DateTime? PublishDate { get; set; }

        public int? PublishedByAdminUserId { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int? ArchivedByAdminUserId { get; set; }

        public bool IsSystemTemplate { get; set; }

        public int ReadLoginSecurityLevelId { get; set; }

        public int WriteLoginSecurityLevelId { get; set; }

        public Template ResponseTemplate { get; set; }

        public LoginSecurityLevel ReadLoginSecurityLevel { get; set; }

        public LoginSecurityLevel WriteLoginSecurityLevel { get; set; }

        private List<TemplateProfile> profiles = new();

        public IReadOnlyCollection<TemplateProfile> Profiles =>
            this.profiles.AsReadOnly();

        private List<TemplateTargetGroup> targetGroups = new();

        public IReadOnlyCollection<TemplateTargetGroup> TargetGroups =>
            this.targetGroups.AsReadOnly();

        public string? Category { get; set; }

        public void Update(
            string name,
            string identityNumber,
            string? category,
            string content,
            int? responseTemplateId,
            bool isSystemTemplate,
            int readLoginSecurityLevelId,
            int writeLoginSecurityLevelId)
        {
            if (this.ArchiveDate != null ||
                this.PublishDate != null)
            {
                throw new DomainValidationException("Cannot edit Pubished/Archived template");
            }

            this.Name = name;
            this.IdentityNumber = identityNumber;
            this.Category = category;
            this.Content = content;
            this.ResponseTemplateId = responseTemplateId;
            this.IsSystemTemplate = isSystemTemplate;
            this.ReadLoginSecurityLevelId = readLoginSecurityLevelId;
            this.WriteLoginSecurityLevelId = writeLoginSecurityLevelId;
        }

        public void Publish(int adminUserId)
        {
            this.PublishDate = DateTime.Now;
            this.PublishedByAdminUserId = adminUserId;
        }

        public void Unpublish(int adminUserId)
        {
            if (this.IsSystemTemplate)
            {
                throw new DomainException("Can't unpublish a system template");
            }

            this.PublishDate = null;
            this.PublishedByAdminUserId = adminUserId;
        }

        public void Archive(int adminUserId)
        {
            this.ArchiveDate = DateTime.Now;
            this.ArchivedByAdminUserId = adminUserId;
        }

        public void UpdateProfilePermissions(
            int[] profileIds,
            bool canSend,
            bool canReceive)
        {
            foreach (int profileId in profileIds)
            {
                TemplateProfile? match =
                    this.Profiles.SingleOrDefault(e => e.ProfileId == profileId);

                if (match != null)
                {
                    match.CanSend = canSend;
                    match.CanReceive = canReceive;
                }
                else
                {
                    this.profiles.Add(
                        new TemplateProfile(
                            profileId,
                            canSend,
                            canReceive));
                }
            }
        }

        public void UpdateTargetGroupPermissions(
            int[] targetGroupIds,
            bool canSend,
            bool canReceive)
        {
            foreach (int targetGroupId in targetGroupIds)
            {
                TemplateTargetGroup? match =
                    this.TargetGroups.SingleOrDefault(e => e.TargetGroupId == targetGroupId);

                if (match != null)
                {
                    match.CanSend = canSend;
                    match.CanReceive = canReceive;
                }
                else
                {
                    this.targetGroups.Add(
                        new TemplateTargetGroup(
                            targetGroupId,
                            canSend,
                            canReceive));
                }
            }
        }

        public void DeleteProfilePermission(int profileId)
        {
            TemplateProfile templateProfile =
                this.Profiles.Single(e => e.ProfileId == profileId);

            this.profiles.Remove(templateProfile);
        }

        public void DeleteTargetGroupPermission(int targetGroupId)
        {
            TemplateTargetGroup templateTargetGroup =
                this.TargetGroups.Single(e => e.TargetGroupId == targetGroupId);

            this.targetGroups.Remove(templateTargetGroup);
        }
    }
}

namespace ED.Domain
{
    class TemplateMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Templates";

            var builder = modelBuilder.Entity<Template>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.TemplateId);
            builder.Property(e => e.TemplateId).ValueGeneratedOnAdd();

            builder.HasOne(e => e.ResponseTemplate)
                .WithMany()
                .HasForeignKey(e => e.ResponseTemplateId)
                .IsRequired(false);

            builder.HasOne(e => e.ReadLoginSecurityLevel)
                .WithMany()
                .HasForeignKey(e => e.ReadLoginSecurityLevelId);

            builder.HasOne(e => e.WriteLoginSecurityLevel)
                .WithMany()
                .HasForeignKey(e => e.WriteLoginSecurityLevelId);

            builder.HasMany(e => e.Profiles)
                .WithOne()
                .HasForeignKey(e => e.TemplateId);

            builder.HasMany(e => e.TargetGroups)
                .WithOne()
                .HasForeignKey(e => e.TemplateId);
        }
    }
}
