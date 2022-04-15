using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ED.Domain
{
    internal class Repository : IRepository
    {
        private DbContext? dbContext;
        private UnitOfWork? unitOfWork;
        protected DbContext DbContext =>
            this.dbContext
            ?? this.unitOfWork?.DbContext
            ?? throw new DomainException("Repository not initialized.");

        public Repository(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // used for testing
        internal Repository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // similar to EFCore's DbSet.FindAsync
        // keep in mind that as of EF Core 3.1 the built in method does not support "includes"
        internal TEntity FindEntity<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
            where TEntity : class
        {
            var e = this.FindEntityOrDefault(entitySet, keyValues, includes);
            if (e == null)
            {
                throw new DomainObjectNotFoundException(typeof(TEntity).Name, keyValues);
            }

            return e;
        }

        protected TEntity? FindEntityOrDefault<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
            where TEntity : class
        {
            var key = this.FindPrimaryKey<TEntity>();

            var e = this.FindTrackedEntityOrDefault<TEntity>(keyValues, key);
            if (e != null)
            {
                // Return the local object if it exists.
                return e;
            }

            // Look in the database
            return this.FindInStoreSingleOrDefault(entitySet, keyValues, key, includes);
        }

        protected async Task<TEntity> FindEntityAsync<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            var e = await this.FindEntityOrDefaultAsync(entitySet, keyValues, includes, ct);
            if (e == null)
            {
                throw new DomainObjectNotFoundException(typeof(TEntity).Name, keyValues);
            }

            return e;
        }

        protected async Task<TEntity?> FindEntityOrDefaultAsync<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            var key = this.FindPrimaryKey<TEntity>();

            var e = this.FindTrackedEntityOrDefault<TEntity>(keyValues, key);
            if (e != null)
            {
                // Return the local object if it exists.
                return e;
            }

            // Look in the database
            return await this.FindInStoreSingleOrDefaultAsync(
                entitySet,
                keyValues,
                key,
                includes,
                ct);
        }

        protected async Task<TEntity> FindEntityAsync<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            var e = await this.FindEntityOrDefaultAsync(
                entitySet,
                predicate,
                includes,
                ct);
            if (e == null)
            {
                throw new DomainObjectNotFoundException(typeof(TEntity).Name);
            }

            return e;
        }

        protected async Task<TEntity?> FindEntityOrDefaultAsync<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            // Look in the database
            return await this.FindInStoreSingleOrDefaultAsync(
                entitySet,
                predicate,
                includes,
                ct);
        }

        protected TEntity FindTrackedEntity<TEntity>(object[] keyValues)
            where TEntity : class
        {
            var e = this.FindTrackedEntityOrDefault<TEntity>(keyValues);
            if (e == null)
            {
                throw new DomainObjectNotFoundException(typeof(TEntity).Name, keyValues);
            }

            return e;
        }

        protected TEntity? FindTrackedEntityOrDefault<TEntity>(object[] keyValues)
            where TEntity : class
        {
            var key = this.FindPrimaryKey<TEntity>();

            return this.FindTrackedEntityOrDefault<TEntity>(keyValues, key);
        }

        protected IEnumerable<TEntity> TrackedEntities<TEntity>() where TEntity : class
        {
            return this.DbContext.ChangeTracker
                .Entries<TEntity>()
                .Select(e => e.Entity);
        }

        protected async Task<TEntity[]> FindEntitiesAsync<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            return await this.FindInStoreMultipleAsync(
                entitySet,
                predicate,
                includes,
                ct);
        }

        protected void CheckVersion(byte[] version1, byte[] version2)
        {
            if (!Enumerable.SequenceEqual(version1, version2))
            {
                throw new DomainUpdateConcurrencyException();
            }
        }

        private IKey FindPrimaryKey<TEntity>()
            where TEntity : class
        {
            return this.DbContext.Model
                .FindEntityType(typeof(TEntity))
                .FindPrimaryKey();
        }

        private TEntity? FindTrackedEntityOrDefault<TEntity>(
            object[] keyValues,
            IKey key)
            where TEntity : class
        {
            var keyProperties = key.Properties;

            if (keyProperties.Count != keyValues.Length)
            {
                if (keyProperties.Count == 1)
                {
                    throw new ArgumentException(
                        CoreStrings.FindNotCompositeKey(
                            typeof(TEntity).ShortDisplayName(),
                            keyValues.Length));
                }

                throw new ArgumentException(
                    CoreStrings.FindValueCountMismatch(
                        typeof(TEntity).ShortDisplayName(),
                        keyProperties.Count,
                        keyValues.Length));
            }

            for (var i = 0; i < keyValues.Length; i++)
            {
                var valueType = keyValues[i].GetType();
                var propertyType = keyProperties[i].ClrType;
                if (valueType != (Nullable.GetUnderlyingType(propertyType) ?? propertyType))
                {
                    throw new ArgumentException(
                        CoreStrings.FindValueTypeMismatch(
                            i,
                            typeof(TEntity).ShortDisplayName(),
                            valueType.ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                }
            }

#pragma warning disable EF1001
            return (((IDbContextDependencies)this.DbContext).StateManager
                .TryGetEntry(key, keyValues)?.Entity as TEntity)
                ?? default(TEntity);
#pragma warning restore EF1001
        }

        private TEntity? FindInStoreSingleOrDefault<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            IKey primaryKey,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
            where TEntity : class
        {
            return this.FindInStoreSingleOrDefault(
                entitySet,
                this.GeneratePredicate<TEntity>(keyValues, primaryKey),
                includes);
        }

        private async Task<TEntity?> FindInStoreSingleOrDefaultAsync<TEntity>(
            DbSet<TEntity> entitySet,
            object[] keyValues,
            IKey primaryKey,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            return await this.FindInStoreSingleOrDefaultAsync(
                entitySet,
                this.GeneratePredicate<TEntity>(keyValues, primaryKey),
                includes,
                ct);
        }

        private Expression<Func<TEntity, bool>> GeneratePredicate<TEntity>(
            object[] keyValues,
            IKey primaryKey)
        {
            var keyProperties = primaryKey.Properties;
            var parameter = Expression.Parameter(typeof(TEntity), "x");

            var predicate = this.GenerateEqualExpression(
                keyProperties[0],
                parameter,
                keyValues[0]);
            for (var i = 1; i < keyProperties.Count; i++)
            {
                predicate = Expression.AndAlso(
                    predicate,
                    this.GenerateEqualExpression(
                        keyProperties[i],
                        parameter,
                        keyValues[i]));
            }

            return (Expression<Func<TEntity, bool>>)Expression.Lambda(predicate, parameter);
        }

        private BinaryExpression GenerateEqualExpression(
            IProperty property,
            ParameterExpression parameter, object k) =>
                Expression.Equal(
                    Expression.Property(parameter, property.Name),
                    Expression.Constant(k, property.ClrType));

        private TEntity? FindInStoreSingleOrDefault<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
            where TEntity : class
        {
            IQueryable<TEntity> queryWithIncludes = entitySet;

            for (int i = 0; i < includes.Length; i++)
            {
                queryWithIncludes = includes[i](queryWithIncludes);
            }

            return queryWithIncludes
                .AsSplitQuery()
                .Where(predicate)
                .SingleOrDefault();
        }

        private async Task<TEntity?> FindInStoreSingleOrDefaultAsync<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            IQueryable<TEntity> queryWithIncludes = entitySet;

            for (int i = 0; i < includes.Length; i++)
            {
                queryWithIncludes = includes[i](queryWithIncludes);
            }

            return await queryWithIncludes
                .AsSplitQuery()
                .Where(predicate)
                .SingleOrDefaultAsync(ct);
        }

        private async Task<TEntity[]> FindInStoreMultipleAsync<TEntity>(
            DbSet<TEntity> entitySet,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes,
            CancellationToken ct)
            where TEntity : class
        {
            IQueryable<TEntity> queryWithIncludes = entitySet;

            for (int i = 0; i < includes.Length; i++)
            {
                queryWithIncludes = includes[i](queryWithIncludes);
            }

            return await queryWithIncludes
                .AsSplitQuery()
                .Where(predicate)
                .ToArrayAsync(ct);
        }
    }
}
