using AutoMapper;
using N4C.Domain;
using N4C.Extensions;
using System.Linq.Expressions;

namespace N4C.Models
{
    public class ServiceConfig<TEntity, TRequest, TResponse> : Config
         where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        public bool NoTracking { get; private set; } = true;
        public bool SplitQuery { get; private set; } = true;
        public bool SqlServer { get; private set; } = true;
        public bool SoftDelete { get; private set; } = true;

        public void SetNoTracking(bool noTracking) => NoTracking = noTracking;
        public void SetSplitQuery(bool splitQuery) => SplitQuery = splitQuery;
        public void SetSqlServer(bool sqlServer) => SqlServer = sqlServer;
        public void SetSoftDelete(bool softDelete) => SoftDelete = softDelete;
        
        public IMappingExpression<TEntity, TRequest> SetRequest() => CreateMap<TEntity, TRequest>();

        public IMappingExpression<TEntity, TResponse> SetResponse() => CreateMap<TEntity, TResponse>()
            .Map(destination => destination.CreateDateS, source => 
                source.CreateDate.HasValue ? $"{source.CreateDate.Value.ToShortDateString()} {source.CreateDate.Value.ToLongTimeString()}" : string.Empty)
            .Map(destination => destination.UpdateDateS, source => 
                source.UpdateDate.HasValue? $"{source.UpdateDate.Value.ToShortDateString()} {source.UpdateDate.Value.ToLongTimeString()}" : string.Empty);

        public List<string> UniqueProperties { get; private set; } = ["Name", "UserName", "Title"];

        public bool PageOrder { get; private set; }

        public List<string> RecordsPerPageCounts { get; private set; } = new List<string>();

        public Dictionary<string, string> OrderExpressions { get; private set; } = new Dictionary<string, string>();

        public void SetUniqueProperties(params Expression<Func<TRequest, object>>[] uniqueProperties)
        {
            UniqueProperties.Clear();
            foreach (var uniqueProperty in uniqueProperties)
            {
                UniqueProperties.Add(uniqueProperty.GetProperty().Name);
            }
        }

        public void SetPageOrder(bool pageOrder, params int[] recordsPerPageCounts)
        {
            PageOrder = pageOrder;
            RecordsPerPageCounts.Clear();
            if (PageOrder)
            {
                if (recordsPerPageCounts.Any())
                    RecordsPerPageCounts.AddRange(recordsPerPageCounts.Select(recordsPerPageCount => recordsPerPageCount.ToString()));
                else
                    RecordsPerPageCounts.AddRange(["5", "10", "25", "50", "100"]);
                RecordsPerPageCounts.Add(Culture == Cultures.TR ? "Tümü" : "All");
            }
        }

        public void SetOrderExpressions(params Expression<Func<TResponse, object>>[] entityProperties)
        {
            Property property;
            var entityPropertyList = ObjectExtensions.GetProperties<TEntity>();
            var descValue = Culture == Cultures.TR ? "Azalan" : "Descending";
            OrderExpressions.Clear();
            foreach (var entityProperty in entityProperties)
            {
                property = entityProperty.GetProperty();
                if (entityPropertyList.Any(entityPropertyItem => entityPropertyItem.Name == property.Name))
                {
                    OrderExpressions.Add(property.Name, property.DisplayName.GetDisplayName(property.Name, Culture));
                    OrderExpressions.Add($"{property.Name}{"DESC"}", $"{property.DisplayName.GetDisplayName(property.Name, Culture)} {descValue}");
                }
            }
        }
    }
}
