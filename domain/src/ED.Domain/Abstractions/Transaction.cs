using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ED.Domain
{
    internal sealed class Transaction : ITransaction
    {
        private bool commited;
        private IDbContextTransaction? transaction;
        private bool childTransaction;

        public Transaction(
            IDbContextTransaction transaction,
            bool childTransaction)
        {
            this.transaction = transaction
                ?? throw new ArgumentNullException(nameof(transaction));
            this.childTransaction = childTransaction;
        }

        public IDbContextTransaction DbContextTransaction
            => this.transaction
                ?? throw new InvalidOperationException("Transaction has already been disposed.");

        public async Task CommitAsync(CancellationToken ct)
        {
            if (this.commited)
            {
                throw new InvalidOperationException("ChildTransaction has already been commited.");
            }

            if (this.transaction == null)
            {
                throw new InvalidOperationException("Transaction has already been disposed.");
            }

            this.commited = true;

            if (!this.childTransaction)
            {
                await this.transaction.CommitAsync(ct);
            }
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            await this.RollbackIntAsync(ct);
        }

        public DbTransaction GetDbTransaction()
        {
            return this.DbContextTransaction.GetDbTransaction();
        }

        public async ValueTask DisposeAsync()
        {
            if (this.transaction != null)
            {
                if (!this.commited)
                {
                    await this.RollbackIntAsync(default);
                }

                if (!this.childTransaction)
                {
                    await this.transaction.DisposeAsync();
                }

                this.transaction = null;
            }
        }

        private async Task RollbackIntAsync(CancellationToken ct)
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Transaction has already been disposed.");
            }

            try
            {
                await this.transaction.RollbackAsync(ct);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // multiple calls to Rollback will throw an exception
                // but this is a valid scenario in out nested transactions implementation
            }
        }
    }
}
