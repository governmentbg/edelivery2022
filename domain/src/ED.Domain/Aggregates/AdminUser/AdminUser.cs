using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class AdminUser
    {
        // EF constructor
        private AdminUser()
        {
            this.AdminsProfile = null!;
        }

        public AdminUser(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            string phone,
            string email,
            string userName,
            string passwordHash,
            int adminUserId)
        {
            this.ConcurrencyStamp = Guid.NewGuid().ToString();
            this.Email = email;
            this.EmailConfirmed = true;
            this.NormalizedEmail = email.ToUpperInvariant();
            this.NormalizedUserName = userName.ToUpperInvariant();
            this.PasswordHash = passwordHash;
            this.PhoneNumber = phone;
            this.PhoneNumberConfirmed = true;
            this.SecurityStamp = Guid.NewGuid().ToString();
            this.UserName = userName;

            this.AdminsProfile = new(
                firstName,
                middleName,
                lastName,
                identifier,
                adminUserId);
        }

        public int Id { get; set; }

        public int AccessFailedCount { get; set; }

        public string? ConcurrencyStamp { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public string? NormalizedEmail { get; set; }

        public string? NormalizedUserName { get; set; }

        public string? PasswordHash { get; set; }

        public string? PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public string? SecurityStamp { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string? UserName { get; set; }

        public AdminsProfile AdminsProfile { get; set; }

        public void Activate()
        {
            this.LockoutEnabled = false;
            this.LockoutEnd = null;

            this.AdminsProfile.Activate();
        }

        public void Deactivate(int adminUserId)
        {
            this.LockoutEnabled = true;
            this.LockoutEnd = DateTime.UtcNow.AddYears(200);

            this.AdminsProfile.Deactivate(adminUserId);
        }

        public void Update(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            string phone,
            string email)
        {
            this.ConcurrencyStamp = Guid.NewGuid().ToString();
            this.Email = email;
            this.EmailConfirmed = true;
            this.NormalizedEmail = email.ToUpperInvariant();
            this.PhoneNumber = phone;
            this.PhoneNumberConfirmed = true;

            this.AdminsProfile.Update(
                firstName,
                middleName,
                lastName,
                identifier);
        }

        public void ChangePassword(string passwordHash)
        {
            this.ConcurrencyStamp = Guid.NewGuid().ToString();
            this.PasswordHash = passwordHash;
            this.SecurityStamp = Guid.NewGuid().ToString();
        }
    }

    class AdminUserMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "AdminUsers";

            var builder = modelBuilder.Entity<AdminUser>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.HasOne(e => e.AdminsProfile)
                .WithOne()
                .HasPrincipalKey<AdminUser>(e => e.Id)
                .HasForeignKey<AdminsProfile>(e => e.Id)
                .IsRequired(true);
        }
    }
}
