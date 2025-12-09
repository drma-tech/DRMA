using System.Linq.Expressions;

namespace DRMA.API.Repository.Core;

/// <summary>
///     Borrowed from:
///     https://docs.microsoft.com/en-us/archive/blogs/meek/linq-to-entities-combining-predicates
/// </summary>
internal static class ExpressionExtensions
{
    internal static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
        Func<Expression, Expression, Expression> merge)
    {
        IDictionary<ParameterExpression, ParameterExpression> map = first.Parameters
            .Select((parameter, index) => (parameter, second: second.Parameters[index]))
            .ToDictionary(p => p.second, p => p.parameter);

        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    internal static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.AndAlso);
    }

    internal static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.And);
    }

    internal static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.Or);
    }
}

internal class ParameterRebinder : ExpressionVisitor
{
    private readonly IDictionary<ParameterExpression, ParameterExpression> _map;

    internal ParameterRebinder(IDictionary<ParameterExpression, ParameterExpression> map)
    {
        _map = map;
    }

    internal static Expression ReplaceParameters(IDictionary<ParameterExpression, ParameterExpression> map,
        Expression exp)
    {
        return new ParameterRebinder(map).Visit(exp);
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (_map.TryGetValue(node, out var replacement)) node = replacement;

        return base.VisitParameter(node);
    }
}