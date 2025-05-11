using AutoMapper;
using N4C.Common;
using N4C.Domain;
using N4C.Extensions;
using System.Linq.Expressions;

namespace N4C.App.Services
{
    public class ServiceConfig<TEntity, TRequest, TResponse> : MapConfig, IServiceConfig where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        public string Culture { get; set; } = Settings.Culture;
        public string TitleTR { get; set; } = "Kayıt";
        public string TitleEN { get; set; } = "Record";
        public bool NoTracking { get; set; } = true;
        public bool SplitQuery { get; set; }
        public string Collation { get; set; } = "Turkish_CI_AS";
        public string TrueHtml { get; set; } = "<i class='bx bx-check fs-3'></i>";
        public string FalseHtml { get; set; } = "<i class='bx bx-x fs-3'></i>";

        public List<string> UniqueProperties { get; private set; } = ["Name", "UserName", "Title"];
        public bool PageOrder { get; private set; }
        public List<string> RecordsPerPageCounts { get; private set; } = new List<string>();
        public Dictionary<string, string> OrderExpressions { get; private set; } = new Dictionary<string, string>();

        public IMappingExpression<TEntity, TResponse> Response => this.Map<TEntity, TResponse>();
        public IMappingExpression<TEntity, TRequest> Request => this.Map<TEntity, TRequest>();

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
