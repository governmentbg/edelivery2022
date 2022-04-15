using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class AdminsProfile
    {
        // EF constructor
        private AdminsProfile()
        {
            this.FirstName = null!;
            this.MiddleName = null!;
            this.LastName = null!;
            this.EGN = null!;
        }

        public AdminsProfile(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            int adminUserId)
        {
            this.FirstName = firstName;
            this.MiddleName = middleName;
            this.LastName = lastName;
            this.EGN = identifier;

            this.CreatedOn = DateTime.Now;
            this.CreatedByAdminUserId = adminUserId;
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedByAdminUserId { get; set; }

        public string EGN { get; set; }

        public DateTime? DisabledOn { get; set; }

        public int? DisabledByAdminUserId { get; set; }

        public string? DisableReason { get; set; }

        public void Activate()
        {
            this.DisabledByAdminUserId = null;
            this.DisabledOn = null;
            this.DisableReason = null;
        }

        public void Deactivate(int adminUserId)
        {
            this.DisabledByAdminUserId = adminUserId;
            this.DisabledOn = DateTime.Now;
            this.DisableReason = string.Empty;
        }

        public void Update(
            string firstName,
            string middleName,
            string lastName,
            string identifier)
        {
            this.FirstName = firstName;
            this.MiddleName = middleName;
            this.LastName = lastName;
            this.EGN = identifier;
        }
    }

    class AdminsProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "AdminsProfiles";

            var builder = modelBuilder.Entity<AdminsProfile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
