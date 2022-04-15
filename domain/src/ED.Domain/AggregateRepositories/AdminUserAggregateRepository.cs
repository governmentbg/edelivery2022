using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class AdminUserAggregateRepository : AggregateRepository<AdminUser>
    {
        public AdminUserAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<AdminUser>, IQueryable<AdminUser>>[] Includes =>
            new Func<IQueryable<AdminUser>, IQueryable<AdminUser>>[]
            {
                (q) => q.Include(e => e.AdminsProfile)
            };
    }
}
