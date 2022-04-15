using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class RecipientGroupAggregateRepository : AggregateRepository<RecipientGroup>
    {
        public RecipientGroupAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<RecipientGroup>, IQueryable<RecipientGroup>>[] Includes =>
            new Func<IQueryable<RecipientGroup>, IQueryable<RecipientGroup>>[]
            {
                (q) => q.Include(e => e.Profiles)
            };
    }
}
