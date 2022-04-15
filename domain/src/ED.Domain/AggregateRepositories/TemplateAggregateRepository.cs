using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class TemplateAggregateRepository : AggregateRepository<Template>
    {
        public TemplateAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<Template>, IQueryable<Template>>[] Includes =>
            new Func<IQueryable<Template>, IQueryable<Template>>[]
            {
                (q) => q
                    .Include(e => e.ResponseTemplate) // TODO: rethink includes
                    .Include(e => e.ReadLoginSecurityLevel)
                    .Include(e => e.WriteLoginSecurityLevel)
                    .Include(e => e.Profiles)
                    .Include(e => e.TargetGroups)
            };
    }
}
