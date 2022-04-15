using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class RecipientGroup
    {
        private const int IndividualRecipientGroupMaxMemberCount = 20;

        // EF constructor
        private RecipientGroup()
        {
            this.Name = null!;
        }

        public RecipientGroup(
            string name,
            int profileId,
            int loginId)
        {
            this.Name = name;
            this.IsPublic = false;
            this.ProfileId = profileId;
            this.CreatedBy = loginId;
            this.ModifiedBy = loginId;

            DateTime now = DateTime.Now;

            this.CreateDate = now;
            this.ModifyDate = now;
        }

        public RecipientGroup(
           string name,
           int adminUserId)
        {
            DateTime now = DateTime.Now;

            this.Name = name;
            this.IsPublic = true;
            this.ProfileId = Profile.SystemProfileId;

            this.CreatedBy = Login.SystemLoginId;
            this.CreatedByAdminUserId = adminUserId;
            this.CreateDate = now;

            this.ModifiedBy = Login.SystemLoginId;
            this.ModifiedByAdminUserId = adminUserId;
            this.ModifyDate = now;
        }

        public int RecipientGroupId { get; set; }

        public int? ProfileId { get; set; }

        public string Name { get; set; }

        public bool IsPublic { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime ModifyDate { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int? ArchivedBy { get; set; }

        public int? CreatedByAdminUserId { get; set; }

        public int? ModifiedByAdminUserId { get; set; }

        public int? ArchivedByAdminUserId { get; set; }

        private List<RecipientGroupProfile> profiles = new();

        public IReadOnlyCollection<RecipientGroupProfile> Profiles =>
            this.profiles.AsReadOnly();

        public void RemoveMember(int profileId, int loginId)
        {
            this.profiles.RemoveAll(e => e.ProfileId == profileId);

            this.ModifiedBy = loginId;
            this.ModifyDate = DateTime.Now;
        }

        public void AddMembers(int[] profileIds, int loginId)
        {
            RecipientGroupProfile[] newProfiles = profileIds
                .Where(e => !this.profiles.Select(e => e.ProfileId).Contains(e))
                .Select(e => new RecipientGroupProfile(e))
                .ToArray(); 

            if (newProfiles.Any())
            {
                this.profiles.AddRange(newProfiles);
                this.ModifiedBy = loginId;
                this.ModifyDate = DateTime.Now;
            }
        }

        public void AddMembersWithLimit(int[] profileIds, int loginId)
        {
            RecipientGroupProfile[] newProfiles;

            int freeSlots = Math.Max(
                0,
                IndividualRecipientGroupMaxMemberCount - this.profiles.Count);

            newProfiles = profileIds
                .Where(e => !this.profiles.Select(e => e.ProfileId).Contains(e))
                .Select(e => new RecipientGroupProfile(e))
                .Take(freeSlots)
                .ToArray();

            if (newProfiles.Any())
            {
                this.profiles.AddRange(newProfiles);
                this.ModifiedBy = loginId;
                this.ModifyDate = DateTime.Now;
            }
        }

        public void Update(string name, int loginId)
        {
            this.Name = name;

            this.ModifiedBy = loginId;
            this.ModifyDate = DateTime.Now;
        }

        public void Archive(int loginId)
        {
            DateTime now = DateTime.Now;

            this.ModifiedBy = loginId;
            this.ModifyDate = now;

            this.ArchivedBy = loginId;
            this.ArchiveDate = now;
        }

        public void RemoveMemberByAdmin(int profileId, int adminUserId)
        {
            this.profiles.RemoveAll(e => e.ProfileId == profileId);

            this.ModifiedBy = Login.SystemLoginId;
            this.ModifiedByAdminUserId = adminUserId;
            this.ModifyDate = DateTime.Now;
        }

        public void AddMembersByAdmin(int[] profileIds, int adminUserId)
        {
            RecipientGroupProfile[] newProfiles = profileIds
                .Where(e => !this.profiles.Select(e => e.ProfileId).Contains(e))
                .Select(e => new RecipientGroupProfile(e))
                .ToArray();

            this.profiles.AddRange(newProfiles);

            this.ModifiedBy = Login.SystemLoginId;
            this.ModifiedByAdminUserId = adminUserId;
            this.ModifyDate = DateTime.Now;
        }

        public void UpdateByAdmin(string name, int adminUserId)
        {
            this.Name = name;

            this.ModifiedBy = Login.SystemLoginId;
            this.ModifiedByAdminUserId = adminUserId;
            this.ModifyDate = DateTime.Now;
        }

        public void ArchiveByAdmin(int adminUserId)
        {
            DateTime now = DateTime.Now;

            this.ModifiedBy = Login.SystemLoginId;
            this.ModifiedByAdminUserId = adminUserId;
            this.ModifyDate = now;

            this.ArchivedBy = Login.SystemLoginId;
            this.ArchivedByAdminUserId = adminUserId;
            this.ArchiveDate = now;
        }
    }

    class RecipientGroupMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "RecipientGroups";

            var builder = modelBuilder.Entity<RecipientGroup>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.RecipientGroupId);
            builder.Property(e => e.RecipientGroupId).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Profiles)
                .WithOne()
                .HasForeignKey(e => e.RecipientGroupId);
        }
    }
}
