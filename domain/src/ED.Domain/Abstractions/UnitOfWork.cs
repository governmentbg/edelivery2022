using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ED.Domain
{
    internal class UnitOfWork : IUnitOfWork
    {
        private bool disposed;
        private Lazy<IDbContextTransactionManager> dbContextTransactionManager;
        private Lazy<DbContext> dbContext;

        public UnitOfWork(
            IDbContextFactory<DbContext> dbContextFactory)
        {
            this.dbContext =
                new Lazy<DbContext>(
                    () => dbContextFactory.CreateDbContext(),
                    isThreadSafe: false);
            this.dbContextTransactionManager =
                new Lazy<IDbContextTransactionManager>(
                    () => this.DbContext.Database.GetService<IDbContextTransactionManager>(),
                    isThreadSafe: false);
        }

        public DbContext DbContext => this.dbContext.Value;

        private IDbContextTransactionManager DbContextTransactionManager
            => this.dbContextTransactionManager.Value;

        public async Task SaveAsync(CancellationToken ct)
        {
            try
            {
                await this.DbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DomainUpdateConcurrencyException();
            }
        }

        public void UseConnection(
            DbConnection connection,
            DbTransaction? transaction = null)
        {
            this.DbContext.Database.SetDbConnection(connection);
            if (transaction != null)
            {
                this.DbContext.Database.UseTransaction(transaction);
            }
        }

        public void UseTransaction(ITransaction transaction)
        {
            transaction = transaction
                ?? throw new ArgumentNullException(nameof(transaction));

            var dbTransaction = transaction.GetDbTransaction();
            var dbConnection = dbTransaction.Connection
                ?? throw new DomainException("The provided transaction is not associated with a connection.");

            this.UseConnection(dbConnection, dbTransaction);
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct)
        {
            var existingTransaction = this.DbContextTransactionManager.CurrentTransaction;
            if (existingTransaction != null)
            {
                return new Transaction(existingTransaction, true);
            }

            // transaction isolation level must not be specified as
            // it is persisted across requests by the connection pool
            return new Transaction(
                await this.DbContext.Database.BeginTransactionAsync(ct),
                false);
        }

        public void Dispose()
        {
            // sync disposal is required as IServiceScope currently does not
            // provide async disposal and working with it will make Autofac
            // to throw an exception that the scope is disposed synchronously
            // but the service only supports asynchronous disposal
            if (!this.disposed)
            {
                if (this.dbContext.IsValueCreated)
                {
                    this.dbContext.Value.Dispose();
                }
                this.disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.disposed)
            {
                if (this.dbContext.IsValueCreated)
                {
                    await this.dbContext.Value.DisposeAsync();
                }
                this.disposed = true;
            }
        }
    }
}
