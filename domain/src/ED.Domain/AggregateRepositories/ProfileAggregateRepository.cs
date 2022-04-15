using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class ProfileAggregateRepository : AggregateRepository<Profile>
    {
        public ProfileAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        protected override Func<IQueryable<Profile>, IQueryable<Profile>>[] Includes =>
            new Func<IQueryable<Profile>, IQueryable<Profile>>[]
            {
                (q) => q.Include(e => e.Blobs),
                (q) => q.Include(e => e.Keys),
                (q) => q.Include(e => e.Individual),
                (q) => q.Include(e => e.LegalEntity),
                (q) => q.Include(e => e.Logins),
                (q) => q.Include(e => e.Address),
                (q) => q.Include(e => e.LoginPermissions),
                (q) => q.Include(e => e.Quota),
            };
    }
}
