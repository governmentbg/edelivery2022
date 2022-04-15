using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfileEsbUser
    {
        // EF constructor
        private ProfileEsbUser()
        {
        }

        public ProfileEsbUser(
            int profileId,
            string oId,
            string clientId,
            int adminUserId)
        {
            this.ProfileId = profileId;
            this.OId = oId;
            this.ClientId = clientId;
            this.ModifiedByAdminUserId = adminUserId;
            this.DateModified = DateTime.Now;
        }

        public void Update(
            string oId,
            string clientId,
            int adminUserId)
        {
            this.OId = oId;
            this.ClientId = clientId;
            this.ModifiedByAdminUserId = adminUserId;
            this.DateModified = DateTime.Now;
        }

        public int ProfileId { get; set; }

        public string? OId { get; set; }

        public string? ClientId { get; set; }

        public DateTime DateModified { get; set; }

        public int? ModifiedByAdminUserId { get; set; }
    }

    class ProfileEsbUserMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ProfileEsbUsers";

            var builder = modelBuilder.Entity<ProfileEsbUser>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.ProfileId);

            // TODO: make one-to-many relationship
            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Profile))
                .WithOne()
                .HasPrincipalKey(typeof(Profile), new string[] { "Id" })
                .HasForeignKey(typeof(ProfileEsbUser), new string[] { "ProfileId" })
                .IsRequired(true);
        }
    }
}
