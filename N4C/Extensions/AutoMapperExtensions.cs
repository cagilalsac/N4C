using AutoMapper;
using AutoMapper.QueryableExtensions;
using N4C.Models;
using System.Linq.Expressions;

namespace N4C.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> Map<TSource, TDestination, TMember>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TDestination, TMember>> destination, Expression<Func<TSource, TMember>> source)
            where TSource : class, new() where TDestination : class, new()
        {
            return mappingExpression.ForMember(destination, options => options.MapFrom(source));
        }

        public static IQueryable<TDestination> Map<TSource, TDestination>(this IQueryable<TSource> query, ServiceConfig serviceConfig = default)
            where TSource : class, new() where TDestination : class, new()
        {
            var mapperConfigurationExpression = new MapperConfigurationExpression();
            mapperConfigurationExpression.CreateMap<TSource, TDestination>();
            if (serviceConfig is not null)
                mapperConfigurationExpression.AddProfile(serviceConfig);
            var mapperConfiguration = new MapperConfiguration(mapperConfigurationExpression);
            return query.ProjectTo<TDestination>(mapperConfiguration);
        }

        public static TDestination Map<TSource, TDestination>(this TSource source, ServiceConfig serviceConfig = default, TDestination destination = default) 
            where TSource : class, new() where TDestination : class, new()
        {
            var mapperConfigurationExpression = new MapperConfigurationExpression();
            mapperConfigurationExpression.CreateMap<TSource, TDestination>();
            if (serviceConfig is not null)
                mapperConfigurationExpression.AddProfile(serviceConfig);
            var mapperConfiguration = new MapperConfiguration(mapperConfigurationExpression);
            var mapper = new Mapper(mapperConfiguration);
            if (destination is null)
                return mapper.Map<TDestination>(source);
            return mapper.Map(source, destination);
        }
    }
}
