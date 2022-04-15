using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IAggregateRepository<TEntity> : IRepository
        where TEntity : class
    {
        Task<TEntity> FindAsync(int id, CancellationToken ct);

        Task<TEntity?> TryFindAsync(int id, CancellationToken ct);

        Task AddAsync(TEntity entity, CancellationToken ct);

        void Remove(TEntity entity, bool forceDetectChanges = false);
    }
}
