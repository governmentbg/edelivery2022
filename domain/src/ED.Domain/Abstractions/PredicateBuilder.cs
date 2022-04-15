using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace ED.Domain
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> True<T>(
            [SuppressMessage("", "CA1801")] T typeInference)
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> True<T>(
            [SuppressMessage("", "CA1801")] IQueryable<T> typeInference)
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> False<T>(
            [SuppressMessage("", "CA1801")] T typeInference)
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> False<T>(
            [SuppressMessage("", "CA1801")] IQueryable<T> typeInference)
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        private static Expression<T> Compose<T>(
            this Expression<T> first,
            Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map =
                first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody =
                ExpressionParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression
            return Expression.Lambda<T>(
                merge(first.Body, secondBody),
                first.Parameters);
        }
    }

    class ExpressionParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ExpressionParameterRebinder(
            Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map =
                map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(
            Dictionary<ParameterExpression, ParameterExpression> map,
            Expression exp)
        {
            return new ExpressionParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (this.map.TryGetValue(p, out ParameterExpression? replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }
}
