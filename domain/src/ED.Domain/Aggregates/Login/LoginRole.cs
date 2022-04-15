using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class LoginRole
    {
        public const int MasterAdministrator = 1;
        public const int Administrator = 2;
        public const int User = 3;

        // EF constructor
        private LoginRole()
        {
        }

        public LoginRole(int roleId)
        {
            this.RoleId = roleId;
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }
    }

    class LoginRoleMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "LoginsRoles";

            var builder = modelBuilder.Entity<LoginRole>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
