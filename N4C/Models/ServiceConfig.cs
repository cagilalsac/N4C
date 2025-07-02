using AutoMapper;
using N4C.Domain;
using N4C.Extensions;
using System.Linq.Expressions;

namespace N4C.Models
{
    public class ServiceConfig<TEntity, TRequest, TResponse> : ServiceConfig
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

        public IMappingExpression<TRequest, TEntity> SetEntity() => CreateMap<TRequest, TEntity>();
        public IMappingExpression<TEntity, TRequest> SetRequest() => CreateMap<TEntity, TRequest>();

        public IMappingExpression<TEntity, TResponse> SetResponse() => CreateMap<TEntity, TResponse>()
            .Map(destination => destination.CreateDate_, source =>
                source.CreateDate.HasValue && source.CreatedBy.HasAny() ? 
                    $"{source.CreatedBy} @ {source.CreateDate.Value.ToShortDateString()} {source.CreateDate.Value.ToLongTimeString()}" : 
                    string.Empty)
            .Map(destination => destination.UpdateDate_, source => 
                source.UpdateDate.HasValue && source.UpdatedBy.HasAny() ? 
                    $"{source.UpdatedBy} @ {source.UpdateDate.Value.ToShortDateString()} {source.UpdateDate.Value.ToLongTimeString()}" : 
                    string.Empty);

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

        public void SetPageOrder(int[] recordsPerPageCounts = default, params Expression<Func<TResponse, object>>[] orderEntityProperties)
        {
            PageOrder = true;
            Property property = null;
            var descValue = Culture == Defaults.TR ? "Azalan" : "Descending";
            var entityPropertyList = ObjectExtensions.GetProperties<TEntity>();
            OrderExpressions.Clear();
            bool entityPropertyFound = true;
            foreach (var entityProperty in orderEntityProperties)
            {
                property = entityProperty.GetProperty();
                if (entityPropertyList.Any(entityPropertyItem => entityPropertyItem.Name == property.Name))
                {
                    OrderExpressions.Add(property.Name, property.DisplayName.GetDisplayName(property.Name, Culture));
                    OrderExpressions.Add($"{property.Name}{"DESC"}", $"{property.DisplayName.GetDisplayName(property.Name, Culture)} {descValue}");
                }
                else
                {
                    entityPropertyFound = false;
                    break;
                }
            }
            if (!entityPropertyFound)
            {
                OrderExpressions.Clear();
                OrderExpressions.Add(string.Empty, Culture == Defaults.TR ? $"{property.Name} sıralanamaz!" : $"{property.Name} can't be ordered!");
            }
            RecordsPerPageCounts.Clear();
            RecordsPerPageCounts.Add(Culture == Defaults.TR ? "Tümü" : "All");
            if (recordsPerPageCounts is null || !recordsPerPageCounts.Any())
                recordsPerPageCounts = Defaults.RecordsPerPageCounts;
            RecordsPerPageCounts.AddRange(recordsPerPageCounts.Select(recordsPerPageCount => recordsPerPageCount.ToString()));
        }
    }
}
