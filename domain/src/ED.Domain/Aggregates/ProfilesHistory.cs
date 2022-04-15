using System;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfilesHistory
    {
        // EF constructor
        private ProfilesHistory()
        {
        }

        public ProfilesHistory(
            int profileId,
            ProfileHistoryAction action,
            int actionLogin,
            string? actionDetails,
            string? ipAddress)
        {
            this.ProfileId = profileId;
            this.Action = action;
            this.ActionLogin = actionLogin;
            this.ActionDate = DateTime.Now;
            this.ActionDetails = actionDetails;
            this.IPAddress = ipAddress;
        }

        public ProfilesHistory(
            int profileId,
            ProfileHistoryAction action,
            int adminUserId)
        {
            this.ProfileId = profileId;
            this.Action = action;
            this.ActionDate = DateTime.Now;
            this.ActionByAdminUserId = adminUserId;
        }

        public static ProfilesHistory CreateInstanceByAdmin(
            int profileId,
            ProfileHistoryAction action,
            int adminUserId,
            string? actionDetails,
            string? ipAddress)
            => new()
            {
                ProfileId = profileId,
                Action = action,
                ActionDate = DateTime.Now,
                ActionByAdminUserId = adminUserId,
                ActionDetails = actionDetails,
                IPAddress = ipAddress,
            };

        public int Id { get; set; }

        public int ProfileId { get; set; }

        public ProfileHistoryAction Action { get; set; }

        public int? ActionLogin { get; set; }

        public DateTime ActionDate { get; set; }

        public string? ActionDetails { get; set; }

        public string? IPAddress { get; set; }

        public int? ActionByAdminUserId { get; set; }

        public static string GenerateAccessDetails(
            ProfileHistoryAction profileHistoryAction,
            Guid electronicSubjectId,
            string electronicSubjectName,
            string details)
        {
            XDocument doc = new(
                new XElement("AccessDetails",
                    new XElement("Action", profileHistoryAction.ToString()),
                    new XElement("UserId", electronicSubjectId.ToString()),
                    new XElement("UserName", new XText(electronicSubjectName))));

            if (!string.IsNullOrEmpty(details))
            {
                doc.Root!.Add(new XElement("ActionDetails", new XText(details)));
            }

            return doc.ToString();
        }
    }

    class ProfilesHistoryMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ProfilesHistory";

            var builder = modelBuilder.Entity<ProfilesHistory>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.Action).HasConversion<string>(); // TODO: needed?

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(ProfilesHistory.ActionLogin))
                .OnDelete(DeleteBehavior.Restrict);

            // TODO: add shadow nav property to adminusers?
        }
    }
}
