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
            bool smsNotificationActive,
            bool smsNotificationOnDeliveryActive,
            bool viberNotificationActive,
            bool viberNotificationOnDeliveryActive,
            string email,
            string phone,
            int accessGrantedBy)
        {
            this.LoginId = loginId;
            this.IsDefault = isDefault;
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.SmsNotificationActive = smsNotificationActive;
            this.SmsNotificationOnDeliveryActive = smsNotificationOnDeliveryActive;
            this.ViberNotificationActive = viberNotificationActive;
            this.ViberNotificationOnDeliveryActive = viberNotificationOnDeliveryActive;
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

        public bool SmsNotificationActive { get; set; }

        public bool SmsNotificationOnDeliveryActive { get; set; }

        public bool ViberNotificationActive { get; set; }

        public bool ViberNotificationOnDeliveryActive { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int AccessGrantedBy { get; set; }

        public int? AccessGrantedByAdminUserId { get; set; }

        public DateTime DateAccessGranted { get; set; }

        public void Update(
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool smsNotificationActive,
            bool smsNotificationOnDeliveryActive,
            bool viberNotificationActive,
            bool viberNotificationOnDeliveryActive,
            string email,
            string phone)
        {
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.SmsNotificationActive = smsNotificationActive;
            this.SmsNotificationOnDeliveryActive = smsNotificationOnDeliveryActive;
            this.ViberNotificationActive = viberNotificationActive;
            this.ViberNotificationOnDeliveryActive = viberNotificationOnDeliveryActive;
            this.Email = email;
            this.Phone = phone;
        }

        public void UpdateByAdmin(
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool smsNotificationActive,
            bool smsNotificationOnDeliveryActive,
            bool viberNotificationActive,
            bool viberNotificationOnDeliveryActive,
            string email,
            string phone,
            int adminUserId)
        {
            this.EmailNotificationActive = emailNotificationActive;
            this.EmailNotificationOnDeliveryActive = emailNotificationOnDeliveryActive;
            this.SmsNotificationActive = smsNotificationActive;
            this.SmsNotificationOnDeliveryActive = smsNotificationOnDeliveryActive;
            this.ViberNotificationActive = viberNotificationActive;
            this.ViberNotificationOnDeliveryActive = viberNotificationOnDeliveryActive;
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
