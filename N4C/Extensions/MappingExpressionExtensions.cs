using AutoMapper;
using System.Linq.Expressions;

namespace N4C.Extensions
{
    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDestination> ForMember<TSource, TDestination, TMember>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TDestination, TMember>> destination, Expression<Func<TSource, TMember>> source)
        {
            return mappingExpression.ForMember(destination, options => options.MapFrom(source));
        }
    }
}
