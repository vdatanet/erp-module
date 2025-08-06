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
}