using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.Domain;

namespace N4C.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, PageOrder pageOrder) where TEntity : Entity, new()
        {
            if (string.IsNullOrWhiteSpace(pageOrder.OrderExpression))
                return query;
            return pageOrder.OrderExpression.EndsWith("DESC") ?
                query.OrderByDescending(entity => EF.Property<object>(entity, pageOrder.OrderExpression.Remove(pageOrder.OrderExpression.Length - 4))) :
                query.OrderBy(entity => EF.Property<object>(entity, pageOrder.OrderExpression));
        }

        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PageOrder pageOrder)
        {
            pageOrder.TotalRecordsCount = query.Count();
            int recordsPerPageCount;
            if (int.TryParse(pageOrder.RecordsPerPageCount, out recordsPerPageCount))
                query = query.Skip((pageOrder.PageNumber - 1) * recordsPerPageCount).Take(recordsPerPageCount);
            return query;
        }

        public static IQueryable<TDestination> Map<TEntity, TDestination>(this IQueryable<TEntity> query, MapperProfile mapperProfile = default) 
            where TEntity : Entity, new() where TDestination : class, new()
        {
            var mapperConfigurationExpression = new MapperConfigurationExpression();
            mapperConfigurationExpression.CreateMap<TEntity, TDestination>();
            if (mapperProfile is not null)
                mapperConfigurationExpression.AddProfile(mapperProfile);
            var mapperConfiguration = new MapperConfiguration(mapperConfigurationExpression);
            return query.ProjectTo<TDestination>(mapperConfiguration);
        }
    }
}
