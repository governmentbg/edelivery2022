using System;
using System.Linq.Expressions;

namespace ED.Domain
{
    class EntityCodeNomsRepository<TEntity, TNomVO> : NomsRepository<TEntity, TEntity, string, TNomVO>
        where TEntity : class
    {
        public EntityCodeNomsRepository(
            UnitOfWork unitOfWork,
            Expression<Func<TEntity, string>> keySelector,
            Expression<Func<TEntity, string>> nameSelector,
            Expression<Func<TEntity, TNomVO>> voSelector,
            Expression<Func<TEntity, decimal>>? orderBySelector = null)
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
