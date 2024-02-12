using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class TicketAggregateRepository : AggregateRepository<Ticket>
    {
        public TicketAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<Ticket>, IQueryable<Ticket>>[] Includes =>
            new Func<IQueryable<Ticket>, IQueryable<Ticket>>[]
            {
                (q) => q.Include(e => e.Statuses)
            };
    }
}
