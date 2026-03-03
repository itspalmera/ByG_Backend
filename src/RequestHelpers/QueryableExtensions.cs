using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ByG_Backend.src.RequestHelpers
{
    public static class QueryableExtensions
    {
        // Tu método de paginación que ya teníamos
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResponse<T>(items, totalItems, pageNumber, pageSize);
        }

        // NUEVO MÉTODO: Ordenamiento Dinámico Genérico
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? orderBy, string defaultSort = "Id")
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                return query.OrderByDynamic(defaultSort, false);

            var parts = orderBy.Trim().Split(':');
            var propertyName = parts[0];
            bool isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            return query.OrderByDynamic(propertyName, isDescending);
        }


        // src/RequestHelpers/QueryableExtensions.cs

public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchTerm, params string[] propertyNames)
{
    if (string.IsNullOrWhiteSpace(searchTerm) || propertyNames == null || propertyNames.Length == 0)
        return query;

    var term = searchTerm.Trim().ToLower();
    var parameter = Expression.Parameter(typeof(T), "x");
    Expression? comparisonExpression = null;

    foreach (var propName in propertyNames)
    {
        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));

        if (property == null || property.PropertyType != typeof(string)) continue;

        // x.PropertyName
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        
        // x.PropertyName.ToLower()
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var toLowerCall = Expression.Call(propertyAccess, toLowerMethod);

        // x.PropertyName.ToLower().Contains(term)
        var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var constant = Expression.Constant(term);
        var containsCall = Expression.Call(toLowerCall, containsMethod, constant);

        // Unir con OR: (Prop1.Contains || Prop2.Contains)
        comparisonExpression = comparisonExpression == null 
            ? containsCall 
            : Expression.OrElse(comparisonExpression, containsCall);
    }

    if (comparisonExpression == null) return query;

    var lambda = Expression.Lambda<Func<T, bool>>(comparisonExpression, parameter);
    return query.Where(lambda);
}     


        private static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string propertyName, bool isDescending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            // Buscamos la propiedad ignorando mayúsculas/minúsculas
            var property = typeof(T).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null) return query; // Si no existe la propiedad, no ordena

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(typeof(Queryable), methodName, 
                new Type[] { typeof(T), property.PropertyType }, 
                query.Expression, Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}