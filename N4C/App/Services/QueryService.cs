using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App.Services.Files;
using N4C.App.Services.Files.Models;
using N4C.Common;
using N4C.Domain;
using N4C.Extensions;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace N4C.App.Services
{
    public abstract class Service<TEntity, TQuery> : Service, IDisposable where TEntity : Entity, new() where TQuery : Response, new()
    {
        private PropertyInfo DeletedProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IDeleted.Deleted));
        private PropertyInfo MainFileProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(FileEntity.MainFile));
        private PropertyInfo OtherFilesProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(FileEntity.OtherFiles));

        protected bool HasDeleted => DeletedProperty is not null;
        protected bool HasFile => MainFileProperty is not null && OtherFilesProperty is not null;

        protected MapperProfile MapperProfile { get; private set; } = new MapperProfile();
        protected string RecordsFound { get; private set; }
        protected string TrueHtml { get; private set; } = "<i class='bx bx-check fs-3'></i>";
        protected string FalseHtml { get; private set; } = "<i class='bx bx-x fs-3'></i>";

        private bool PageOrder { get; set; }
        private List<string> RecordsPerPageCounts { get; set; } = new List<string>();
        private Dictionary<string, string> OrderExpressions { get; set; } = new Dictionary<string, string>();

        protected FileService FileService { get; set; }

        protected readonly IDb _db;

        protected Service(IDb db, HttpService httpService, ILogger<Service> logger) : base(httpService, logger)
        {
            _db = db;
            SetRecordsPerPageCounts(5, 10, 25, 50, 100);
        }

        protected override void SetCulture(string culture)
        {
            base.SetCulture(culture);
            SetRecordsPerPageCounts(5, 10, 25, 50, 100);
            SetTitle(Culture == Cultures.TR ? "Kayıt" : "Record");
        }

        protected override void SetTitle(string tr, string en = default)
        {
            base.SetTitle(tr, en);
            RecordsFound = Culture == Cultures.TR ? $"{Title.ToLower()} bulundu" : $"{Title.ToLower()} record(s) found";
        }

        protected void SetTrueHtml(string trueHtml)
        {
            TrueHtml = trueHtml;
        }

        protected void SetFalseHtml(string falseHtml)
        {
            FalseHtml = falseHtml;
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

        protected void SetOrderExpressions(params Expression<Func<TQuery, object>>[] entityProperties)
        {
            Property property;
            var entityPropertyList = ObjectExtensions.GetProperties<TEntity>();
            var descValue = Culture == Cultures.TR ? "Azalan" : "Descending";
            foreach (var entityProperty in entityProperties)
            {
                property = entityProperty.GetProperty();
                if (entityPropertyList.Any(entityPropertyItem => entityPropertyItem.Name == property.Name) && !OrderExpressions.ContainsKey(property.Name))
                {
                    OrderExpressions.Add(property.Name, property.DisplayName.GetDisplayName(property.Name, Culture));
                    OrderExpressions.Add($"{property.Name}{"DESC"}", $"{property.DisplayName.GetDisplayName(property.Name, Culture)} {descValue}");
                }
            }
        }

        protected virtual IQueryable<TEntity> Query(Action<MapperProfile> mapperProfile = default)
        {
            if (mapperProfile is not null)
                mapperProfile.Invoke(MapperProfile);
            var query = _db.Set<TEntity>().AsNoTracking();
            if (HasDeleted)
                query = query.Where(entity => EF.Property<bool>(entity, DeletedProperty.Name) == false);
            return query;
        }

        protected TEntity Query(int id)
        {
            return Query().SingleOrDefault(entity => entity.Id == id) ?? new TEntity();
        }

        protected TEntity Query(string guid)
        {
            return Query().SingleOrDefault(entity => entity.Guid == guid) ?? new TEntity();
        }

        public virtual async Task<Result<List<TQuery>>> GetList(CancellationToken cancellationToken = default)
        {
            List<TQuery> list = null;
            try
            {
                list = await Query().Map<TEntity, TQuery>(MapperProfile).ToListAsync(cancellationToken);
                if (list.Count > 0)
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
                    return Success(list, $"{list.Count} {RecordsFound}");
                }
                return Error(list, HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.GetList(): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<List<TQuery>>> GetList(PageOrder pageOrder, CancellationToken cancellationToken = default)
        {
            List<TQuery> list = null;
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
                    list = await Query().OrderBy(pageOrder).Paginate(pageOrder).Map<TEntity, TQuery>(MapperProfile).ToListAsync(cancellationToken);
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

        public virtual async Task<Result<TQuery>> GetItem(int id, CancellationToken cancellationToken = default)
        {
            TQuery item = null;
            try
            {
                item = await Query().Map<TEntity, TQuery>(MapperProfile).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                if (item is null)
                    return Error(item, HttpStatusCode.NotFound);
                if (HasFile && item is FileResponse)
                {
                    FileService = new FileService(HttpService, Logger);
                    FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                }
                return Success(item, $"1 {RecordsFound}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.GetItem(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public async Task GetExcel()
        {
            FileService = new FileService(HttpService, Logger);
            var result = await GetList();
            if (result.Success)
                FileService.GetExcel(result.Data);
        }

        public Result<FileResponse> GetFile(string filePath)
        {
            FileService = new FileService(HttpService, Logger);
            return FileService.GetFile(filePath);
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
