#nullable enable
using DevExpress.Data.Filtering;
using DevExpress.Data.Linq;
using DevExpress.Xpo;
using System.Linq.Expressions;

namespace erp.Blazor.Server.Helpers;

public static class SearchHelper
{
    public static IQueryable<T> ApplySearchFilter<T>(
        this IQueryable<T> query,
        string? search,
        params Expression<Func<T, string>>[] propertySelectors) where T : class
    {
        if (string.IsNullOrWhiteSpace(search) || propertySelectors.Length == 0)
        {
            return query;
        }

        var conditions = new List<CriteriaOperator>();
        foreach (var selector in propertySelectors)
        {
            var propertyName = (selector.Body as MemberExpression)?.Member.Name;
            if (propertyName != null)
            {
                conditions.Add(CriteriaOperator.Parse($"Contains([{propertyName}], ?)", search));
            }
        }

        var combinedCriteria = CriteriaOperator.Or(conditions.ToArray());
        var xpQuery = query as XPQuery<T>;
        if (xpQuery == null)
        {
            return query;
        }

        var converter = new CriteriaToExpressionConverter();
        var parameter = Expression.Parameter(typeof(T), "x");
        var convertedExpression = converter.Convert(parameter, combinedCriteria);
        var lambda = Expression.Lambda<Func<T, bool>>(convertedExpression, parameter);

        return xpQuery.Where(lambda);
    }
}