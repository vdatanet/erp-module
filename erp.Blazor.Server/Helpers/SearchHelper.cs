using System.Linq.Expressions;

namespace erp.Blazor.Server.Helpers
{
    public static class SearchHelper
    {
        public static IQueryable<T> ApplySearchFilter<T>(
            this IQueryable<T> query, 
            string? search,
            params Expression<Func<T, string>>[] propertySelectors)
        {
            if (string.IsNullOrWhiteSpace(search) || !propertySelectors.Any())
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
            var searchConstant = Expression.Constant(search);

            Expression? combinedExpression = null;
            foreach (var selector in propertySelectors)
            {
                var memberAccess = Expression.Invoke(selector, parameter);
                var condition = Expression.Call(memberAccess, containsMethod!, searchConstant);
                
                combinedExpression = combinedExpression == null 
                    ? condition 
                    : Expression.OrElse(combinedExpression, condition);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression!, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySearchFilter<T>(
            this IQueryable<T> query,
            string? search,
            params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(search) || !propertyNames.Any())
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
            var searchConstant = Expression.Constant(search);

            Expression? combinedExpression = null;
            foreach (var propertyName in propertyNames)
            {
                var property = Expression.Property(parameter, propertyName);
                var condition = Expression.Call(property, containsMethod!, searchConstant);
                
                combinedExpression = combinedExpression == null 
                    ? condition 
                    : Expression.OrElse(combinedExpression, condition);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression!, parameter);
            return query.Where(lambda);
        }
    }
}