using System.Linq.Expressions;
using System.Reflection;
using System;

namespace J4JSoftware.Utilities;

public static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<TContainer, TProp>(
        this Expression<Func<TContainer, TProp>> expression
    ) =>
        expression.Body switch
        {
            null => throw new ArgumentNullException( nameof( expression ) ),
            UnaryExpression ue when ue.Operand is MemberExpression me => (PropertyInfo) me.Member,
            MemberExpression me => (PropertyInfo) me.Member,
            _ => throw new ArgumentException( $"The expression isn't a valid property. [ {expression} ]" )
        };
}