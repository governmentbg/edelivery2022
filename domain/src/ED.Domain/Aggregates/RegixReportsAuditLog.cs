using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class RegixReportsAuditLog
    {
        // EF constructor
        private RegixReportsAuditLog()
        {
            this.Data = null!;
        }

        public RegixReportsAuditLog(
            Guid token,
            string data,
            int createdByLoginId,
            int createdForProfileId,
            DateTime dateCreated)
        {
            this.Token = token;
            this.Data = data.Length > 990 ? data.Substring(0, 990) : data;
            this.CreatedByLoginId = createdByLoginId;
            this.CreatedForProfileId = createdForProfileId;
            this.DateCreated = dateCreated;
        }

        public int Id { get; set; }

        public Guid Token { get; set; }

        public string Data { get; set; }

        public int CreatedByLoginId { get; set; }

        public int CreatedForProfileId { get; set; }

        public DateTime DateCreated { get; set; }
    }

    class RegixReportsAuditLogMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "RegixReportsAuditLog";

            var builder = modelBuilder.Entity<RegixReportsAuditLog>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
