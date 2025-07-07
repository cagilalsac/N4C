using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;
using System.Linq.Expressions;

namespace N4C.Services
{
    public abstract class Service<TEntity, TRequest, TResponse> : Service, IDisposable
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected override ServiceConfig Config { get; set; } = new ServiceConfig<TEntity, TRequest, TResponse>();

        private ServiceConfig<TEntity, TRequest, TResponse> _serviceConfig;

        private bool _relationsFound;
        private bool _validated;
        private bool _querySet;

        private DbContext Db { get; }

        protected Service(DbContext db, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<Service> logger)
            : base(httpContextAccessor, httpClientFactory, logger)
        {
            Db = db;
            _serviceConfig = Config as ServiceConfig<TEntity, TRequest, TResponse>;
            Query();
        }

        protected virtual IQueryable<TEntity> Query(Action<ServiceConfig<TEntity, TRequest, TResponse>> config = default)
        {
            if (config is not null)
            {
                config.Invoke(_serviceConfig);
                Set(_serviceConfig.Culture, _serviceConfig.TitleTR, _serviceConfig.TitleEN.HasNotAny("Record") == "Record" ? typeof(TEntity).Name : _serviceConfig.TitleEN);
            }
            var query = Db.Set<TEntity>().AsQueryable();
            if (_querySet)
            {
                query = query.OrderByDescending(entity => entity.UpdateDate).ThenByDescending(entity => entity.CreateDate)
                    .Where(entity => (EF.Property<bool?>(entity, nameof(Entity.Deleted)) ?? false) == false);
                if (_serviceConfig.NoTracking)
                    query = query.AsNoTracking();
                if (_serviceConfig.SqlServer && _serviceConfig.SplitQuery)
                    query = query.AsSplitQuery();
            }
            _querySet = true;
            return query;
        }

        protected List<TEntity> GetEntities(Expression<Func<TEntity, bool>> predicate = default) => predicate is null ? Query().ToList() : Query().Where(predicate).ToList();

        protected TEntity GetEntity(Expression<Func<TEntity, bool>> predicate) => GetEntities(predicate).SingleOrDefault();

        protected TEntity GetEntity(TRequest request)
        {
            TEntity item = null;
            try
            {
                item = GetEntity(entity => entity.Id == request.Id) ?? new TEntity();
            }
            catch (Exception exception)
            {
                LogError($"ServiceException: {exception.Message}");
            }
            return item;
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse(CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Query().Map<TEntity, TResponse>(_serviceConfig).ToListAsync(cancellationToken);
                GetFiles(list);
                return Success(list);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse(PageOrderRequest request, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            var query = Query();
            try
            {
                if (_serviceConfig.PageOrder)
                {
                    GetPageOrderSession(request);
                    request.Page.RecordsPerPageCounts = _serviceConfig.RecordsPerPageCounts;
                    var order = new Order()
                    {
                        Expression = request.OrderExpression.HasNotAny(string.Empty),
                        Expressions = _serviceConfig.OrderExpressions
                    };
                    if (!request.Page.RecordsPerPageCounts.Contains(request.Page.RecordsPerPageCount.HasNotAny(Culture == Defaults.TR ? "Tümü" : "All")))
                        return Error(list, $"Geçersiz sayfadaki kayıt sayısı değeri! Geçerli değerler: {string.Join(", ", request.Page.RecordsPerPageCounts)}",
                            $"Invalid records per page count value! Valid values: {string.Join(", ", request.Page.RecordsPerPageCounts)}");
                    if (order.Expression.HasAny() && order.Expressions.Any() && !order.Expressions.ContainsKey(order.Expression))
                        return Error(list, $"Geçersiz sıralama değeri! Geçerli değerler: {string.Join(", ", order.Expressions.Keys)}",
                            $"Invalid order value! Valid values: {string.Join(", ", order.Expressions.Keys)}");
                    list = await query.OrderBy(order.Expression).Paginate(request.Page).Map<TEntity, TResponse>(_serviceConfig).ToListAsync(cancellationToken);
                    SetPageOrderSession(request.Page, order);
                    GetFiles(list);
                    return Success(list, request.Page, order);
                }
                return await GetResponse(cancellationToken);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public async Task<Result<List<TResponse>>> GetResponse(string pageNumber, string recordsPerPageCount, string orderExpression = default, CancellationToken cancellationToken = default)
        {
            int pageOrderRequestPageNumber;
            if (int.TryParse(pageNumber, out pageOrderRequestPageNumber))
            {
                return await GetResponse(new PageOrderRequest()
                {
                    Page = new Page()
                    {
                        Number = pageOrderRequestPageNumber,
                        RecordsPerPageCount = recordsPerPageCount
                    },
                    OrderExpression = orderExpression
                }, cancellationToken);
            }
            return await GetResponse(cancellationToken);
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Query().Where(predicate).Map<TEntity, TResponse>(_serviceConfig).ToListAsync(cancellationToken);
                GetFiles(list);
                return Success(list);
            }
            catch (Exception exception)
            {
                return Result(exception, list);
            }
        }

        public virtual async Task<Result<TResponse>> GetResponse(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await Query().Map<TEntity, TResponse>(_serviceConfig).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                GetFiles(item);
                return Success(item);
            }
            catch (Exception exception)
            {
                return Result(exception, item);
            }
        }

        public virtual async Task<Result<TRequest>> GetRequest(int? id = default, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                var query = Query();
                if (id.HasValue)
                {
                    var entity = await query.SingleOrDefaultAsync(entity => entity.Id == id.Value, cancellationToken);
                    item = entity.Map(_serviceConfig, item);
                    GetFiles(item, entity);
                }
                else
                {
                    item = new TRequest();
                }
                return Success(item);
            }
            catch (Exception exception)
            {
                return Result(exception, item);
            }
        }

        public virtual async Task<Result<TRequest>> GetRequest(string guid, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                var entity = await Query().SingleOrDefaultAsync(entity => entity.Guid == guid, cancellationToken);
                item = entity.Map(_serviceConfig, item);
                GetFiles(item, entity);
                return Success(item);
            }
            catch (Exception exception)
            {
                return Result(exception, item);
            }
        }

        public Result<T> GetRequest<T>() where T : Request, new()
        {
            var item = new T();
            return Success(item);
        }

        protected virtual Result<TRequest> Validate(TRequest request, ModelStateDictionary modelState = default)
        {
            try
            {
                var query = Query();
                Property entityProperty, requestProperty;
                string collation = "Turkish_CI_AS";
                string modelStateErrors = Validate(modelState).Message;
                string uniquePropertyError = string.Empty;
                if (_serviceConfig.SqlServer)
                {
                    foreach (var uniqueProperty in _serviceConfig.UniqueProperties)
                    {
                        entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                        requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                        if (entityProperty is not null && requestProperty is not null && query.Any(entity => entity.Id != request.Id &&
                            EF.Functions.Collate(EF.Property<string>(entity, entityProperty.Name), collation) == EF.Functions.Collate((requestProperty.Value ?? "").ToString(), collation).Trim()))
                        {
                            if (Culture == Defaults.TR)
                                uniquePropertyError = $"{requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} değerine sahip {_serviceConfig.Title.ToLower()} bulunmaktadır!";
                            else
                                uniquePropertyError = $"{_serviceConfig.Title} with the same value for {requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} exists!";
                            break;
                        }
                    }
                }
                return Validated(request, modelStateErrors, uniquePropertyError);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        protected void Validate<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities.HasAny())
                _relationsFound = ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)) && relationalEntities.Any();
        }

        protected void Update<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities.HasAny() && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
                Db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
        }

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities.HasAny() && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
            {
                if (!_serviceConfig.SoftDelete)
                    Db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
            }
        }

        protected virtual async Task<Result<TRequest>> Create(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_validated)
                {
                    var validationResult = Validate(request);
                    if (!validationResult.Success)
                        return Result(validationResult, request);
                }
                var entity = request.Map<TRequest, TEntity>(_serviceConfig).Trim();
                entity.Guid = Guid.NewGuid().ToString();
                entity.CreateDate = DateTime.Now;
                entity.CreatedBy = GetUserName().HasNotAny(Defaults.User);
                var fileResult = CreateFiles(request, entity);
                if (!fileResult.Success)
                    return Result(fileResult, request);
                Db.Set<TEntity>().Add(entity);
                if (save)
                {
                    await Save(cancellationToken);
                    request.Id = entity.Id;
                    request.Guid = entity.Guid;
                }
                return Created(request);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Create(TRequest request, ModelStateDictionary modelState, bool save = true, CancellationToken cancellationToken = default)
        {
            var validationResult = Validate(request, modelState);
            if (!validationResult.Success)
                return Result(validationResult, request);
            _validated = true;
            return await Create(request, save, cancellationToken);
        }

        public async Task<Result<TRequest>> Create(ApiRequest<TRequest> apiRequest, ModelStateDictionary modelState, bool save = true, CancellationToken cancellationToken = default)
        {
            var request = apiRequest.Request;
            if (request is FileRequest)
            {
                (request as FileRequest).MainFormFile = apiRequest.MainFormFile;
                (request as FileRequest).OtherFormFiles = apiRequest.OtherFormFiles;
            }
            return await Create(request, modelState, save, cancellationToken);
        }

        protected async Task<Result<TEntity>> Update(TEntity entity, bool save = true, CancellationToken cancellationToken = default)
        {
            Db.Set<TEntity>().Update(entity);
            if (save)
            {
                try
                {
                    await Save(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Error(entity, NotFound);
                }
            }
            return Updated(entity);
        }

        protected virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_validated)
                {
                    var validationResult = Validate(request);
                    if (!validationResult.Success)
                        return Result(validationResult, request);
                }
                var entity = Db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return Error(request, NotFound);
                var guid = entity.Guid;
                var createDate = entity.CreateDate;
                var createdBy = entity.CreatedBy;
                entity = request.Map(_serviceConfig, entity).Trim();
                entity.Guid = guid;
                entity.CreateDate = createDate;
                entity.CreatedBy = createdBy;
                entity.UpdateDate = DateTime.Now;
                entity.UpdatedBy = GetUserName();
                var fileResult = UpdateFiles(request, entity);
                if (!fileResult.Success)
                    return Result(fileResult, request);
                return Result(await Update(entity, save, cancellationToken), request);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Update(TRequest request, ModelStateDictionary modelState, bool save = true, CancellationToken cancellationToken = default)
        {
            var validationResult = Validate(request, modelState);
            if (!validationResult.Success)
                return Result(validationResult, request);
            _validated = true;
            return await Update(request, save, cancellationToken);
        }

        public async Task<Result<TRequest>> Update(ApiRequest<TRequest> apiRequest, ModelStateDictionary modelState, bool save = true, CancellationToken cancellationToken = default)
        {
            var request = apiRequest.Request;
            if (request is FileRequest)
            {
                (request as FileRequest).MainFormFile = apiRequest.MainFormFile;
                (request as FileRequest).OtherFormFiles = apiRequest.OtherFormFiles;
            }
            return await Update(request, modelState, save, cancellationToken);
        }

        public virtual async Task<Result<TRequest>> Delete(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_relationsFound)
                    return Error(request, RelationsFound);
                var entity = Db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return Error(request, NotFound);
                if (_serviceConfig.SoftDelete)
                {
                    entity.Deleted = true;
                    entity.UpdateDate = DateTime.Now;
                    entity.UpdatedBy = GetUserName();
                    await Update(entity, false, cancellationToken);
                }
                else
                {
                    var fileResult = DeleteFiles(request, entity);
                    if (!fileResult.Success)
                        return Result(fileResult, request);
                    Db.Set<TEntity>().Remove(entity);
                }
                if (save)
                    await Save(cancellationToken);
                return Deleted(request);
            }
            catch (Exception exception)
            {
                return Result(exception, request);
            }
        }

        public async Task<Result<TRequest>> Delete(int id) => await Delete(new TRequest() { Id = id });

        protected Result<TRequest> CreateFiles(TRequest request, TEntity entity)
        {
            if (request is FileRequest && entity is FileEntity)
            {
                var fileRequest = request as FileRequest;
                var validationResult = ValidateOtherFiles(fileRequest?.OtherFormFiles);
                if (validationResult.Success)
                {
                    var mainFileResult = CreateFile(fileRequest.MainFormFile);
                    if (mainFileResult.Success)
                    {
                        var fileEntity = entity as FileEntity;
                        fileEntity.MainFile = mainFileResult.Data.MainFile;
                        var otherFilesResult = CreateFiles(fileRequest.OtherFormFiles);
                        if (otherFilesResult.Success)
                            fileEntity.OtherFiles = GetOtherFilePaths(otherFilesResult.Data, 1);
                        else
                            return Result(otherFilesResult, request);
                    }
                    else
                    {
                        return Result(mainFileResult, request);
                    }
                }
            }
            return Success(request);
        }

        protected Result<TRequest> UpdateFiles(TRequest request, TEntity entity = default)
        {
            if (request is FileRequest)
            {
                if (entity is null)
                {
                    entity = Db.Set<TEntity>().Find(request.Id);
                    if (entity is null)
                        return Error(request, NotFound);
                }
                if (entity is FileEntity)
                {
                    var fileRequest = request as FileRequest;
                    var fileEntity = entity as FileEntity;
                    var validationResult = ValidateOtherFiles(fileRequest?.OtherFormFiles, fileEntity.OtherFiles);
                    if (validationResult.Success)
                    {
                        var mainFileResult = UpdateFile(fileRequest.MainFormFile, fileEntity.MainFile);
                        if (mainFileResult.Success)
                        {
                            fileEntity.MainFile = mainFileResult.Data.MainFile;
                            var orderInitialValue = 1;
                            if (fileEntity.OtherFiles.HasAny())
                            {
                                var lastOtherFile = fileEntity.OtherFiles.Order().Last();
                                orderInitialValue = GetFileOrder(lastOtherFile) + 1;
                            }
                            var otherFilesResult = CreateFiles(fileRequest.OtherFormFiles);
                            if (otherFilesResult.Success)
                            {
                                if (otherFilesResult.Data.HasAny())
                                    fileEntity.OtherFiles = GetOtherFilePaths(otherFilesResult.Data, orderInitialValue);
                            }
                            else
                            {
                                return Result(otherFilesResult, request);
                            }
                        }
                        else
                        {
                            return Result(mainFileResult, request);
                        }
                    }
                }
            }
            return Success(request);
        }

        protected Result<TRequest> DeleteFiles(TRequest request, TEntity entity, string filePath = default)
        {
            Result<FileResponse> mainFileResult;
            Result<List<FileResponse>> otherFilesResult;
            if (request is FileRequest && entity is FileEntity)
            {
                var fileEntity = entity as FileEntity;
                if (filePath.HasNotAny())
                {
                    mainFileResult = DeleteFile(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Result(mainFileResult, request);
                    fileEntity.MainFile = null;
                    otherFilesResult = DeleteFiles(fileEntity.OtherFiles);
                    if (!otherFilesResult.Success)
                        return Result(otherFilesResult, request);
                    fileEntity.OtherFiles = null;
                }
                else if (filePath == fileEntity.MainFile)
                {
                    mainFileResult = DeleteFile(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Result(mainFileResult, request);
                    fileEntity.MainFile = null;
                }
                else
                {
                    mainFileResult = DeleteFile(filePath);
                    if (!mainFileResult.Success)
                        return Result(mainFileResult, request);
                    filePath = fileEntity.OtherFiles.SingleOrDefault(otherFile => $"/{GetFileFolder(otherFile)}/{GetFileName(otherFile)}" == filePath);
                    if (filePath.HasAny())
                    {
                        fileEntity.OtherFiles.Remove(filePath);
                        if (!fileEntity.OtherFiles.HasAny())
                            fileEntity.OtherFiles = null;
                    }
                }
                Db.Set<TEntity>().Update(entity);
            }
            return Success(request);
        }

        public async Task<Result<TRequest>> DeleteFiles(int id, string filePath = default, CancellationToken cancellationToken = default)
        {
            var request = new TRequest() { Id = id };
            var entity = Db.Set<TEntity>().Find(request.Id);
            if (entity is null)
                return Error(request, NotFound);
            var deleteResult = DeleteFiles(request, entity, filePath);
            if (!deleteResult.Success)
                return Result(deleteResult, request);
            await Save(cancellationToken);
            return Result(deleteResult, request, "Dosya başarıyla silindi", "File deleted successfully");
        }

        protected void GetFiles(TResponse item)
        {
            if (item is FileResponse && item is not null)
                GetOtherFilePaths((item as FileResponse).OtherFiles);
        }

        protected void GetFiles(List<TResponse> list)
        {
            if (list.HasAny())
            {
                foreach (var item in list)
                {
                    GetFiles(item);
                }
            }
        }

        protected void GetFiles(TRequest item, TEntity entity)
        {
            if (item is FileRequest && item is not null && entity is FileEntity && entity is not null)
                (item as FileRequest).MainFile = (entity as FileEntity).MainFile;
        }

        public async Task GetExcel(CancellationToken cancellationToken = default)
        {
            var result = await GetResponse(cancellationToken);
            if (result.Success)
                GetExcel(result.Data);
        }

        protected async Task Save(CancellationToken cancellationToken = default)
        {
            try
            {
                await Db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                LogError($"ServiceException: {exception.Message}");
            }
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
