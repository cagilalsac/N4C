using AutoMapper;
using N4C.App;
using System.Linq.Expressions;

namespace N4C.Extensions
{
    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDestination> Map<TSource, TDestination>(this MapConfig config)
        {
            return config.CreateMap<TSource, TDestination>();
        }

        public static IMappingExpression<TSource, TDestination> Property<TSource, TDestination, TMember>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TDestination, TMember>> destination, Expression<Func<TSource, TMember>> source)
        {
            return mappingExpression.ForMember(destination, options => options.MapFrom(source));
        }
    }
}
