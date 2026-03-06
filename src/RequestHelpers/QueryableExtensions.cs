using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ByG_Backend.src.RequestHelpers
{
    /// <summary>
    /// Extensiones estáticas para <see cref="IQueryable{T}"/>.
    /// Proporciona capacidades de búsqueda global, ordenamiento dinámico por reflexión y paginación asíncrona.
    /// Estas funciones permiten construir consultas complejas en la base de datos de forma flexible.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Convierte una consulta en una respuesta paginada, aplicando límites de seguridad en el tamaño de página.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <param name="query">Consulta base de Entity Framework.</param>
        /// <param name="pageNumber">Índice de página solicitado.</param>
        /// <param name="pageSize">Cantidad de registros por página (máximo 100).</param>
        /// <returns>Objeto <see cref="PagedResponse{T}"/> con los datos y metadatos de paginación.</returns>
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<T>(items, totalItems, pageNumber, pageSize);
        }

        /// <summary>
        /// Aplica ordenamiento dinámico basado en una cadena de texto (ej: "Nombre:desc").
        /// </summary>
        /// <param name="orderBy">Cadena con formato "propiedad:orden" (asc por defecto).</param>
        /// <param name="defaultSort">Propiedad de ordenamiento si el parámetro es nulo.</param>
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? orderBy, string defaultSort = "Id")
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                return query.OrderByDynamic(defaultSort, false);

            var parts = orderBy.Trim().Split(':');
            var propertyName = parts[0];
            bool isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            return query.OrderByDynamic(propertyName, isDescending);
        }

        /// <summary>
        /// Realiza una búsqueda global ("Contains") en múltiples propiedades de tipo string de forma dinámica.
        /// </summary>
        /// <remarks>
        /// Construye un árbol de expresiones que se traduce a una cláusula WHERE (x.Prop1.ToLower().Contains(term) OR x.Prop2.ToLower().Contains(term)).
        /// </remarks>
        /// <param name="searchTerm">Texto a buscar.</param>
        /// <param name="propertyNames">Lista de nombres de propiedades donde se realizará la búsqueda.</param>
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

                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
                var toLowerCall = Expression.Call(propertyAccess, toLowerMethod);

                var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
                var constant = Expression.Constant(term);
                var containsCall = Expression.Call(toLowerCall, containsMethod, constant);

                comparisonExpression = comparisonExpression == null 
                    ? containsCall 
                    : Expression.OrElse(comparisonExpression, containsCall);
            }

            if (comparisonExpression == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(comparisonExpression, parameter);
            return query.Where(lambda);
        }     

        /// <summary>
        /// Método privado que utiliza Reflexión y Expression Trees para invocar OrderBy o OrderByDescending.
        /// </summary>
        private static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string propertyName, bool isDescending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null) return query;

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