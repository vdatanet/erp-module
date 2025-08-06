#nullable enable
using System.Linq.Expressions;

namespace erp.Blazor.Server.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        if (page > 0 && pageSize > 0) query = query.Skip((page - 1) * pageSize).Take(pageSize);
        return query;
    }

    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? search,
        Expression<Func<T, string>> propertySelector)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var parameter = propertySelector.Parameters[0];
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

        var property = Expression.Call(propertySelector.Body, toLowerMethod);
        var searchConstant = Expression.Constant(search.ToLower());
        var contains = Expression.Call(property, containsMethod, searchConstant);

        var lambda = Expression.Lambda<Func<T, bool>>(contains, parameter);
        query = query.Where(lambda);
        return query;
    }
}