using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);
        DbTransaction GetDbTransaction();
    }
}
