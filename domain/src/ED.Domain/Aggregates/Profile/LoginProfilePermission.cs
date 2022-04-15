using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class LoginProfilePermission
    {
        // EF constructor
        private LoginProfilePermission()
        {
        }

        public LoginProfilePermission(
            int loginId,
            LoginProfilePermissionType permission,
            int? templateId)
        {
            this.LoginId = loginId;
            this.Permission = permission;
            this.TemplateId = templateId;
        }

        public int LoginId { get; set; }

        public int ProfileId { get; set; }

        public int LoginProfilePermissionId { get; set; }

        public LoginProfilePermissionType Permission { get; set; }

        public int? TemplateId { get; set; }
    }

    class LoginProfilePermissionMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "LoginProfilePermissions";

            var builder = modelBuilder.Entity<LoginProfilePermission>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => new { e.LoginId, e.ProfileId, e.LoginProfilePermissionId });
            builder.Property(e => e.LoginProfilePermissionId).ValueGeneratedOnAdd();

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(LoginProfile))
                .WithMany()
                .HasForeignKey(nameof(LoginProfilePermission.LoginId), nameof(LoginProfilePermission.ProfileId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
