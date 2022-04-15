using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    internal class AggregateRepository<TEntity>
        : BaseAggregateRepository<TEntity>, IAggregateRepository<TEntity>
        where TEntity : class
    {
        public AggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public Task<TEntity> FindAsync(int id, CancellationToken ct)
        {
            return this.FindEntityAsync(
                this.DbContext.Set<TEntity>(),
                new object[] { id },
                this.Includes,
                ct);
        }

        public Task<TEntity?> TryFindAsync(int id, CancellationToken ct)
        {
            return this.FindEntityOrDefaultAsync(
                this.DbContext.Set<TEntity>(),
                new object[] { id },
                this.Includes,
                ct);
        }
    }
}
