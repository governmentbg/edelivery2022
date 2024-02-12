using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class MessageTranslationAggregateRepository : AggregateRepository<MessageTranslation>
    {
        public MessageTranslationAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<MessageTranslation>, IQueryable<MessageTranslation>>[] Includes =>
            new Func<IQueryable<MessageTranslation>, IQueryable<MessageTranslation>>[]
            {
                (q) => q.Include(e => e.Requests)
            };
    }
}
