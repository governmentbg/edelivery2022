using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task SaveAsync(CancellationToken ct);
        Task<ITransaction> BeginTransactionAsync(CancellationToken ct);
        void UseConnection(DbConnection connection, DbTransaction? transaction = null);
        void UseTransaction(ITransaction transaction);
    }
}
