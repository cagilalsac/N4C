using Microsoft.EntityFrameworkCore;
using N4C.Domain;
using N4C.Models;

namespace N4C.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, string expression) where TEntity : Entity, new()
        {
            if (expression.HasNotAny())
                return query;
            return expression.EndsWith("DESC") ?
                query.OrderByDescending(entity => EF.Property<object>(entity, expression.Remove(expression.Length - 4))) :
                query.OrderBy(entity => EF.Property<object>(entity, expression));
        }

        public static IQueryable<TEntity> Paginate<TEntity>(this IQueryable<TEntity> query, Page page) where TEntity : Entity, new()
        {
            query = query.AsSingleQuery();
            page.TotalRecordsCount = query.Count();
            int recordsPerPageCount;
            if (int.TryParse(page.RecordsPerPageCount, out recordsPerPageCount))
                query = query.Skip((page.Number - 1) * recordsPerPageCount).Take(recordsPerPageCount);
            return query;
        }
    }
}
