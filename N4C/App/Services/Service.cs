using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace N4C.App.Services
{
    public abstract class Service<TEntity, TRequest, TResponse> : Application<TEntity, TRequest, TResponse>
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        private bool PageOrder { get; set; }
        private List<string> RecordsPerPageCounts { get; set; }
        private Dictionary<string, string> OrderExpressions { get; set; }
        
        public string TrueHtml { get; private set; }
        public string FalseHtml { get; private set; }

        protected Service(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            RecordsPerPageCounts = ["5", "10", "25", "50", "100", Culture == Cultures.TR ? "Tümü" : "All"];
            OrderExpressions = new Dictionary<string, string>();
            TrueHtml = "<i class='bx bx-check fs-3'></i>";
            FalseHtml = "<i class='bx bx-x fs-3'></i>";
        }

        protected void SetPageOrder(bool pageOrder)
        {
            PageOrder = pageOrder;
        }

        protected void SetRecordsPerPageCounts(params int[] recordsPerPageCounts)
        {
            if (recordsPerPageCounts.Any())
            {
                RecordsPerPageCounts.Clear();
                foreach (var recordsPerPageCount in recordsPerPageCounts)
                {
                    RecordsPerPageCounts.Add(recordsPerPageCount.ToString());
                }
                RecordsPerPageCounts.Add(Culture == Cultures.TR ? "Tümü" : "All");
            }
        }

        protected void SetOrderExpressions(params Expression<Func<TRequest, object>>[] orderExpressionProperties)
        {
            Property property;
            var descValue = Culture == Cultures.TR ? "Azalan" : "Descending";
            foreach (var orderExpressionProperty in orderExpressionProperties)
            {
                property = orderExpressionProperty.GetProperty();
                if (!OrderExpressions.ContainsKey(property.Name))
                {
                    OrderExpressions.Add(property.Name, property.DisplayName.GetDisplayName(property.Name, Culture));
                    OrderExpressions.Add($"{property.Name}{"DESC"}", $"{property.DisplayName.GetDisplayName(property.Name, Culture)} {descValue}");
                }
            }
        }

        public void SetTrueHtml(string trueHtml)
        {
            TrueHtml = trueHtml;
        }

        public void SetFalseHtml(string falseHtml)
        {
            FalseHtml = falseHtml;
        }

        public async Task<Result<List<TResponse>>> GetList(PageOrder pageOrder, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                if (PageOrder)
                {
                    if (Settings.SessionExpirationInMinutes > 0 && pageOrder.Session)
                    {
                        var pageOrderFromSession = HttpService.GetSession<PageOrder>(nameof(PageOrder));
                        if (pageOrderFromSession is not null)
                        {
                            pageOrder.PageNumber = pageOrderFromSession.PageNumber;
                            pageOrder.RecordsPerPageCount = pageOrderFromSession.RecordsPerPageCount;
                            pageOrder.OrderExpression = pageOrderFromSession.OrderExpression;
                        }
                    }
                    pageOrder.RecordsPerPageCounts = RecordsPerPageCounts;
                    pageOrder.OrderExpressions = OrderExpressions;
                    if (pageOrder.OrderExpressions.Any() && string.IsNullOrWhiteSpace(pageOrder.OrderExpression))
                        pageOrder.OrderExpression = pageOrder.OrderExpressions.FirstOrDefault().Key;
                    list = await Data().OrderBy(pageOrder).Paginate(pageOrder).ProjectTo<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                    if (Settings.SessionExpirationInMinutes > 0)
                        HttpService.SetSession(nameof(PageOrder), pageOrder);
                    if (pageOrder.TotalRecordsCount > 0)
                    {
                        if (HasFile)
                        {
                            FileService = new FileService(HttpService, Logger);
                            foreach (var item in list)
                            {
                                if (item is FileResponse)
                                    FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                            }
                        }
                        return Success(list, $"{pageOrder.TotalRecordsCount} {RecordsFound}");
                    }
                    return Error(list, HttpStatusCode.NotFound);
                }
                pageOrder.PageNumber = 0;
                return await GetList(cancellationToken);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.GetList(PageNumber = {pageOrder.PageNumber}, RecordsPerPageCount = {pageOrder.RecordsPerPageCount}, " +
                    $"OrderExpression = {pageOrder.OrderExpression}): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public Result<TRequest> GetItemForCreate()
        {
            return Success(new TRequest());
        }

        public Result<TRequest> GetItemForEdit(int id, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                var entity = Data(new TRequest() { Id = id });
                if (entity is null)
                    return Error(item, HttpStatusCode.NotFound);
                item = entity.Map<TEntity, TRequest>(MapperProfile);
                if (HasFile && item is FileRequest)
                    (item as FileRequest)._MainFile = (entity as FileEntity).MainFile;
                return Success(item);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.GetItemForEdit(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public Result<TResponse> GetItemForDelete(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                return GetItem(id, cancellationToken).Result;
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.GetItemForDelete(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }
    }
}
