using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Login
    {
        public const int SystemLoginId = 1;

        // EF constructor
        private Login()
        {
            this.UserName = null!;
            this.ElectronicSubjectName = null!;
        }

        public Login(
            Guid loginSubjectId,
            string loginName,
            string email,
            string phone,
            string userName)
        {
            this.ElectronicSubjectId = loginSubjectId;
            this.ElectronicSubjectName = loginName;
            this.Email = email;
            this.PhoneNumber = phone;
            this.UserName = userName;
            this.LockoutEnabled = false;
            this.TwoFactorEnabled = false;
            this.CanSendOnBehalfOf = false;
            this.IsActive = true;
            this.PasswordHash = string.Empty;
            this.SecurityStamp = Guid.NewGuid().ToString();

            this.roles.Add(new LoginRole(LoginRole.User));
        }

        public Login(
            Guid loginSubjectId,
            string loginName,
            string email,
            string phone,
            string userName,
            string certificateThumbPrint,
            string pushNotificationsUrl,
            bool canSendOnBehalfOf)
        {
            this.ElectronicSubjectId = loginSubjectId;
            this.ElectronicSubjectName = loginName;
            this.Email = email;
            this.PhoneNumber = phone;
            this.UserName = userName;
            this.LockoutEnabled = false;
            this.TwoFactorEnabled = false;
            this.CertificateThumbprint = certificateThumbPrint;
            this.PushNotificationsUrl = pushNotificationsUrl;
            this.CanSendOnBehalfOf = canSendOnBehalfOf;
            this.IsActive = true;
        }

        public int Id { get; set; }

        public string UserName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? PasswordHash { get; set; }

        public string? SecurityStamp { get; set; }

        public string? PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public Guid ElectronicSubjectId { get; set; }

        public string ElectronicSubjectName { get; set; }

        public bool IsActive { get; set; }

        public string? CertificateThumbprint { get; set; }

        public bool? CanSendOnBehalfOf { get; set; }

        public string? PushNotificationsUrl { get; set; }

        private List<LoginRole> roles = new();

        public IReadOnlyCollection<LoginRole> Roles =>
            this.roles.AsReadOnly();

        public void Update(string email, string phone)
        {
            this.Email = email;
            this.PhoneNumber = phone;
        }

        public void UpdateIntegrationData(
            string? certificateThumbPrint,
            string? pushNotificationsUrl,
            bool? canSendOnBehalfOf)
        {
            this.CertificateThumbprint = certificateThumbPrint;
            this.PushNotificationsUrl = pushNotificationsUrl;
            this.CanSendOnBehalfOf = canSendOnBehalfOf;
        }

        public void UpdateNames(string fullName)
        {
            this.ElectronicSubjectName = fullName;
        }

        public void Deactivate()
        {
            if (!this.IsActive)
            {
                throw new DomainValidationException($"The operation cannot be performed on logins that are already inactive");
            }

            this.IsActive = false;
        }

        public void Activate()
        {
            if (this.IsActive)
            {
                throw new DomainValidationException($"The operation cannot be performed on logins that are already active");
            }

            this.IsActive = true;
        }
    }

    class LoginMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Logins";

            var builder = modelBuilder.Entity<Login>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Roles)
                .WithOne()
                .HasForeignKey(e => e.UserId);
        }
    }
}
