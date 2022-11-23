using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface INomsRepository<TEntity, TKey, TNomVO> : IRepository
        where TEntity : class
    {
        Task<TNomVO?> GetNomAsync(TKey nomValueId, CancellationToken ct);

        Task<IEnumerable<TNomVO>> GetNomsAsync(string term, int offset, int? limit, CancellationToken ct);
    }
}
