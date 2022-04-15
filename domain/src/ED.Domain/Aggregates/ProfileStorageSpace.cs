using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class ProfileStorageSpace
    {
        // EF constructor
        private ProfileStorageSpace()
        {
        }

        public int ProfileId { get; set; }

        public long UsedStorageSpace { get; set; }
    }

    class ProfileStorageSpaceMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var viewName = "ProfileStorageSpace_Indexed";

            var builder = modelBuilder.Entity<ProfileStorageSpace>();
            builder.HasNoKey();

            builder.ToView(viewName, schema);
        }
    }
}
