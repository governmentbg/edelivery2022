using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfileKey
    {
        // EF constructor
        private ProfileKey()
        {
            this.Provider = null!;
            this.KeyName = null!;
            this.OaepPadding = null!;
        }

        public ProfileKey(
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            this.Provider = provider;
            this.KeyName = keyName;
            this.OaepPadding = oaepPadding;
            this.IssuedAt = issuedAt;
            this.ExpiresAt = expiresAt;
            this.IsActive = true;
        }

        public int ProfileKeyId { get; set; }

        public int ProfileId { get; set; }

        public string Provider { get; set; }

        public string KeyName { get; set; }

        public string OaepPadding { get; set; }

        public DateTime IssuedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; }
    }

    class ProfileKeyMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ProfileKeys";

            var builder = modelBuilder.Entity<ProfileKey>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.ProfileKeyId);
            builder.Property(e => e.ProfileKeyId).ValueGeneratedOnAdd();
        }
    }
}
