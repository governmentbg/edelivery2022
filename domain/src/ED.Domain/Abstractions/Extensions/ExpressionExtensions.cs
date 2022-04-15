using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ED.Domain
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T1, bool>> AndEquals<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2>> prop,
            T2 value)
            where T2 : class
        {
            if (value == null)
            {
                return expr;
            }

            return expr.AndPropertyEquals(prop, value);
        }

        public static Expression<Func<T1, bool>> AndEquals<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2>> prop,
            T2? value)
            where T2 : struct
        {
            if (!value.HasValue)
            {
                return expr;
            }

            return expr.AndPropertyEquals(prop, value.Value);
        }

        public static Expression<Func<T1, bool>> AndEquals<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2?>> prop,
            T2? value)
            where T2 : struct
        {
            if (!value.HasValue)
            {
                return expr;
            }

            return expr.AndPropertyEquals(prop, value);
        }

        public static Expression<Func<T, bool>> AndStringMatches<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop,
            string value,
            bool exactMatch)
        {
            if (string.IsNullOrEmpty(value))
            {
                return expr;
            }

            return exactMatch ?
                expr.AndPropertyEquals(prop, value) :
                expr.AndStringContainsInternal(prop, value);
        }

        public static Expression<Func<T, bool>> AndStringContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop,
            string? value)
        {
            if (value == null)
            {
                return expr;
            }

            return expr.AndStringContainsInternal(prop, value);
        }

        public static Expression<Func<T, bool>> AndAnyStringContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop1,
            Expression<Func<T, string>> prop2,
            string? value)
        {
            if (value == null)
            {
                return expr;
            }

            return expr.And(
                StringContainsExpression(prop1, value)
                .Or(StringContainsExpression(prop2, value)));
        }

        public static Expression<Func<T, bool>> AndAnyStringContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop1,
            Expression<Func<T, string>> prop2,
            Expression<Func<T, string>> prop3,
            string? value)
        {
            if (value == null)
            {
                return expr;
            }

            return expr.And(
                StringContainsExpression(prop1, value)
                .Or(StringContainsExpression(prop2, value))
                .Or(StringContainsExpression(prop3, value)));
        }

        public static Expression<Func<T, bool>> AndAnyStringContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop1,
            Expression<Func<T, string>> prop2,
            Expression<Func<T, string>> prop3,
            Expression<Func<T, string>> prop4,
            string? value)
        {
            if (value == null)
            {
                return expr;
            }

            return expr.And(
                StringContainsExpression(prop1, value)
                .Or(StringContainsExpression(prop2, value))
                .Or(StringContainsExpression(prop3, value))
                .Or(StringContainsExpression(prop4, value)));
        }

        public static Expression<Func<T, bool>> AndAnyStringContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop1,
            Expression<Func<T, string>> prop2,
            Expression<Func<T, string>> prop3,
            Expression<Func<T, string>> prop4,
            Expression<Func<T, string>> prop5,
            string? value)
        {
            if (value == null)
            {
                return expr;
            }

            return expr.And(
                StringContainsExpression(prop1, value)
                .Or(StringContainsExpression(prop2, value))
                .Or(StringContainsExpression(prop3, value))
                .Or(StringContainsExpression(prop4, value))
                .Or(StringContainsExpression(prop5, value)));
        }

        public static Expression<Func<T, bool>> AndStringCollectionContains<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, IEnumerable<string>>> prop,
            string value)
        {
            if (value == null)
            {
                return expr;
            }

            var valueArr = value.Split(',').Select(r => r.Trim().ToLower());

            foreach (var val in valueArr)
            {
                expr = expr.And(
                    Expression.Lambda<Func<T, bool>>(
                        Expression.Call(
                        typeof(Enumerable),
                        "Contains",
                        new[] { typeof(string) },
                        prop.Body,
                        Expression.Constant(val)),
                        prop.Parameters));
            }

            return expr;
        }

        public static Expression<Func<T, bool>> AndCollectionContains<T, TProp>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, IEnumerable<TProp>>> prop,
            TProp value)
        {
            expr = expr.And(
                Expression.Lambda<Func<T, bool>>(
                    Expression.Call(
                    typeof(Enumerable),
                    "Contains",
                    new[] { typeof(TProp) },
                    prop.Body,
                    Expression.Constant(value)),
                    prop.Parameters));

            return expr;
        }

        public static Expression<Func<T, bool>> AndDateGreaterThanOrEqual<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, DateTime?>> prop,
            DateTime? value)
        {
            if (!value.HasValue)
            {
                return expr;
            }

            return expr.AndPropertyGreaterThanOrEqual(prop, value.Value.Date);
        }

        public static Expression<Func<T, bool>> AndDateLessThanOrEqual<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, DateTime?>> prop,
            DateTime? value)
        {
            if (!value.HasValue)
            {
                return expr;
            }

            return expr.AndPropertyLessThanOrEqual(
                prop,
                value.Value.AddDays(1).AddMilliseconds(-1));
        }

        public static Expression<Func<T1, bool>> AndPropertyEquals<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2>> prop, T2 value)
        {
            return expr.And(
                Expression.Lambda<Func<T1, bool>>(
                    Expression.Equal(
                        prop.Body,
                        Expression.Constant(value, typeof(T2))),
                    prop.Parameters));
        }

        public static bool IsTrueLambdaExpr(this Expression expr)
        {
            if (!(expr is LambdaExpression))
            {
                return false;
            }

            var body = ((LambdaExpression)expr).Body;

            if (!(body is ConstantExpression))
            {
                return false;
            }

            var value = ((ConstantExpression)body).Value;

            if (!(value is bool))
            {
                return false;
            }

            return (bool)value;
        }

        private static Expression<Func<T, bool>> AndStringContainsInternal<T>(
            this Expression<Func<T, bool>> expr,
            Expression<Func<T, string>> prop,
            string value)
        {
            return expr.And(StringContainsExpression(prop, value));
        }

        private static Expression<Func<T, bool>> StringContainsExpression<T>(
            Expression<Func<T, string>> prop,
            string value)
        {
            return Expression.Lambda<Func<T, bool>>(
                    Expression.Call(
                        prop.Body,
                        "Contains",
                        null,
                        Expression.Constant(value)),
                    prop.Parameters);
        }

        private static Expression<Func<T1, bool>> AndPropertyGreaterThanOrEqual<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2>> prop,
            T2 value)
        {
            return expr.And(
                Expression.Lambda<Func<T1, bool>>(
                    Expression.GreaterThanOrEqual(
                        prop.Body,
                        Expression.Constant(value, typeof(T2))),
                    prop.Parameters));
        }

        private static Expression<Func<T1, bool>> AndPropertyLessThanOrEqual<T1, T2>(
            this Expression<Func<T1, bool>> expr,
            Expression<Func<T1, T2>> prop,
            T2 value)
        {
            return expr.And(
                Expression.Lambda<Func<T1, bool>>(
                    Expression.LessThanOrEqual(
                        prop.Body,
                        Expression.Constant(value, typeof(T2))),
                    prop.Parameters));
        }
    }
}
