using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class TargetGroupAggregateRepository : AggregateRepository<TargetGroup>
    {
        public TargetGroupAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<TargetGroup>, IQueryable<TargetGroup>>[] Includes =>
            new Func<IQueryable<TargetGroup>, IQueryable<TargetGroup>>[]
            {
                (q) => q.Include(e => e.Matrices)
            };
    }
}
