using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfileQuota
    {
        public const int DefaultStorageQuotaInMb = 1 * 1024; // 1GB in MB
        public const int DefaultStorageQuotaInBytes = 1 * 1024 * 1024 * 1024; // 1GB in Bytes

        // EF constructor
        private ProfileQuota()
        {
        }

        public ProfileQuota(
            int? storageQuotaInMb,
            int adminUserId)
        {
            this.StorageQuotaInMb = storageQuotaInMb;
            this.ModifiedByAdminUserId = adminUserId;
            this.DateModified = DateTime.Now;
        }

        public void Update(
            int? storageQuotaInMb,
            int adminUserId)
        {
            this.StorageQuotaInMb = storageQuotaInMb;
            this.ModifiedByAdminUserId = adminUserId;
            this.DateModified = DateTime.Now;
        }

        public int ProfileId { get; set; }

        public int? StorageQuotaInMb { get; set; }

        public DateTime DateModified { get; set; }

        public int? ModifiedByAdminUserId { get; set; }
    }

    class ProfileQuotaMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "ProfileQuotas";

            var builder = modelBuilder.Entity<ProfileQuota>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.ProfileId);
        }
    }
}
