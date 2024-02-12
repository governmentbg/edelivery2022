using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    class NomsRepository<TEntity, TQuery, TKey, TNomVO> : Repository, INomsRepository<TEntity, TKey, TNomVO>
            where TEntity : class
    {
        protected Expression<Func<TQuery, TKey>> keySelector;
        protected Expression<Func<TQuery, string>> nameSelector;
        protected Expression<Func<TQuery, TNomVO>> voSelector;
        protected Expression<Func<TQuery, decimal>>? orderBySelector;

        public NomsRepository(
            UnitOfWork unitOfWork,
            Expression<Func<TQuery, TKey>> keySelector,
            Expression<Func<TQuery, string>> nameSelector,
            Expression<Func<TQuery, TNomVO>> voSelector,
            Expression<Func<TQuery, decimal>>? orderBySelector = null)
            : base(unitOfWork)
        {
            this.keySelector = keySelector;
            this.nameSelector = nameSelector;
            this.voSelector = voSelector;
            this.orderBySelector = orderBySelector;
        }

        public virtual async Task<TNomVO?> GetNomAsync(TKey nomValueId, CancellationToken ct)
        {
            if (EqualityComparer<TKey>.Default.Equals(nomValueId, default))
            {
                throw new ArgumentException("Filtering by the default value for nomValueId is not allowed.");
            }

            Expression<Func<TQuery, bool>> predicate =
                PredicateBuilder.True<TQuery>()
                .AndPropertyEquals(this.keySelector, nomValueId);

            return await this.GetQuery()
                .Where(predicate)
                .Select(this.voSelector)
                .SingleOrDefaultAsync(ct);
        }

        public virtual async Task<IEnumerable<TNomVO>> GetNomsAsync(
            string term,
            int offset,
            int? limit,
            CancellationToken ct)
        {
            var query = this.GetNameFilteredQuery(term);

            query = this.orderBySelector == null ?
                query.OrderBy(this.nameSelector) :
                query.OrderBy(this.orderBySelector);

            return await query
                .WithOffsetAndLimit(offset, limit)
                .Select(this.voSelector)
                .ToListAsync(ct);
        }

        protected virtual IQueryable<TQuery> GetQuery()
        {
            return this.DbContext.Set<TEntity>().Cast<TQuery>();
        }

        protected virtual IQueryable<TQuery> GetNameFilteredQuery(string term)
        {
            Expression<Func<TQuery, bool>> predicate =
                PredicateBuilder.True<TQuery>()
                .AndStringContains(this.nameSelector, term);

            return this.GetQuery().Where(predicate);
        }
    }
}
