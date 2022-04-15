using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    internal abstract class BaseAggregateRepository<TEntity> : Repository
        where TEntity : class
    {
        public BaseAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken ct)
        {
            await this.DbContext.Set<TEntity>().AddAsync(entity, ct);
        }

        public virtual void Remove(TEntity entity, bool forceDetectChanges = false)
        {
            // This is required when deleting an aggregate that is referenced by
            // a child in another aggregate. Children in our domain are deleted
            // by removing them from the parent, thus triggering ef core's
            // "deleting orphans" feature. But this doesn't happen until after
            // we've tried to remove the aggregate, which will throw an error
            // because the child is still considered unchnaged. Forcing a change
            // detection will correctly mark the child as removed, and will
            // allow the aggregate to be removed.
            if (forceDetectChanges)
            {
                this.DbContext.ChangeTracker.DetectChanges();
            }

            this.DbContext.Set<TEntity>().Remove(entity);
        }

        #region Protected methods

        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>[] Includes
        {
            get
            {
                return Array.Empty<Func<IQueryable<TEntity>, IQueryable<TEntity>>>();
            }
        }

        protected async Task<TEntity> FindEntityAsync(
            object[] keyValues,
            CancellationToken ct)
        {
            return await this.FindEntityAsync(
                this.DbContext.Set<TEntity>(),
                keyValues,
                this.Includes,
                ct);
        }

        protected async Task<TEntity?> FindEntityOrDefaultAsync(
            object[] keyValues,
            CancellationToken ct)
        {
            return await this.FindEntityOrDefaultAsync(
                this.DbContext.Set<TEntity>(),
                keyValues,
                this.Includes,
                ct);
        }

        protected async Task<TEntity> FindEntityAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct)
        {
            return await this.FindEntityAsync(
                this.DbContext.Set<TEntity>(),
                predicate,
                this.Includes,
                ct);
        }

        protected async Task<TEntity?> FindEntityOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct)
        {
            return await this.FindEntityOrDefaultAsync(
                this.DbContext.Set<TEntity>(),
                predicate,
                this.Includes,
                ct);
        }

        protected async Task<TEntity[]> FindEntitiesAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct)
        {
            return await this.FindEntitiesAsync(
                this.DbContext.Set<TEntity>(),
                predicate,
                this.Includes,
                ct);
        }

        #endregion
    }
}
