using System;
using System.Linq.Expressions;

namespace ED.Domain
{
    class EntityNomsRepository<TEntity, TNomVO> : NomsRepository<TEntity, TEntity, int, TNomVO>
            where TEntity : class
    {
        public EntityNomsRepository(
            UnitOfWork unitOfWork,
            Expression<Func<TEntity, int>> keySelector,
            Expression<Func<TEntity, string>> nameSelector,
            Expression<Func<TEntity, TNomVO>> voSelector)
            : base(
                unitOfWork,
                keySelector,
                nameSelector,
                voSelector)
        {
        }

        public EntityNomsRepository(
            UnitOfWork unitOfWork,
            Expression<Func<TEntity, int>> keySelector,
            Expression<Func<TEntity, string>> nameSelector,
            Expression<Func<TEntity, TNomVO>> voSelector,
            Expression<Func<TEntity, decimal>> orderBySelector)
            : base(
                unitOfWork,
                keySelector,
                nameSelector,
                voSelector,
                orderBySelector)
        {
        }
    }
}
