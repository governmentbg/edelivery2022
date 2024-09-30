using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class LoginProfile
    {
        // EF constructor
        private LoginProfile()
        {
            this.Email = null!;
            this.Phone = null!;
        }

        public LoginProfile(
            int loginId,
            bool isDefault,
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool phoneNotificationActive,
            bool phoneNotificationOnDeliveryActive,
            string email,
            string phone,
            int accessGrantedBy)
        {
            this.LoginId = loginId;
            this.IsDefault = isDefault;
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.PhoneNotificationActive = phoneNotificationActive;
            this.PhoneNotificationOnDeliveryActive = phoneNotificationOnDeliveryActive;
            this.Email = email;
            this.Phone = phone;
            this.AccessGrantedBy = accessGrantedBy;
            this.DateAccessGranted = DateTime.Now;
        }

        public int LoginId { get; set; }

        public int ProfileId { get; set; }

        public bool IsDefault { get; set; }

        public bool EmailNotificationActive { get; set; }

        public bool EmailNotificationOnDeliveryActive { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int AccessGrantedBy { get; set; }

        public int? AccessGrantedByAdminUserId { get; set; }

        public DateTime DateAccessGranted { get; set; }

        public bool PhoneNotificationActive { get; set; }

        public bool PhoneNotificationOnDeliveryActive { get; set; }

        public void Update(
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool phoneNotificationActive,
            bool phoneNotificationOnDeliveryActive,
            string email,
            string phone)
        {
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.PhoneNotificationActive = phoneNotificationActive;
            this.PhoneNotificationOnDeliveryActive = phoneNotificationOnDeliveryActive;
            this.Email = email;
            this.Phone = phone;
        }

        public void UpdateByAdmin(
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool phoneNotificationActive,
            bool phoneNotificationOnDeliveryActive,
            string email,
            string phone,
            int adminUserId)
        {
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.PhoneNotificationActive = phoneNotificationActive;
            this.PhoneNotificationOnDeliveryActive = phoneNotificationOnDeliveryActive;
            this.Email = email;
            this.Phone = phone;

            this.AccessGrantedByAdminUserId = adminUserId; // TODO: not correct
        }

        public void Sync(string email, string phone)
        {
            this.Email = email;
            this.Phone = phone;
        }
    }

    class LoginProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "LoginsProfiles";

            var builder = modelBuilder.Entity<LoginProfile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.LoginId, e.ProfileId });

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Profile))
                .WithMany()
                .HasForeignKey(nameof(LoginProfile.ProfileId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
