using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class LoginSecurityLevel
    {
        // EF constructor
        public LoginSecurityLevel()
        {
            this.Name = null!;
        }

        public int LoginSecurityLevelId { get; set; }

        public string Name { get; set; }

        public int NumValue { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedByAdminUserId { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int? ArchivedByAdminUserId { get; set; }
    }

    class LoginSecurityLevelMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "LoginSecurityLevels";

            var builder = modelBuilder.Entity<LoginSecurityLevel>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.LoginSecurityLevelId);
            builder.Property(e => e.LoginSecurityLevelId).ValueGeneratedOnAdd();
        }
    }
}
