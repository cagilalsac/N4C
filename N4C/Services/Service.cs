using Microsoft.AspNetCore.Http;
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
        protected override Config Config { get; set; } = new ServiceConfig<TEntity, TRequest, TResponse>();

        protected ServiceConfig<TEntity, TRequest, TResponse> ServiceConfig => Config as ServiceConfig<TEntity, TRequest, TResponse>;

        private bool RelationsFound { get; set; }

        private DbContext Db { get; }

        protected Service(DbContext db, IHttpContextAccessor httpContextAccessor, ILogger<Service> logger) : base(httpContextAccessor, logger)
        {
            Db = db;
            Set(null);
        }

        protected virtual IQueryable<TEntity> SetQuery(Action<ServiceConfig<TEntity, TRequest, TResponse>> config)
        {
            if (config is not null)
            {
                config.Invoke(ServiceConfig);
                Set(ServiceConfig.Culture, ServiceConfig.TitleTR, (ServiceConfig.TitleEN ?? "Record") == "Record" ? typeof(TEntity).Name : ServiceConfig.TitleEN);
            }
            var query = ServiceConfig.NoTracking ? Db.Set<TEntity>().AsNoTracking() : Db.Set<TEntity>();
            query = query.OrderByDescending(entity => entity.UpdateDate).ThenByDescending(entity => entity.CreateDate);
            if (ServiceConfig.SqlServer && ServiceConfig.SplitQuery)
                query = query.AsSplitQuery();
            return query.Where(entity => (EF.Property<bool?>(entity, nameof(Entity.Deleted)) ?? false) == false);
        }

        protected TEntity GetEntity(TRequest request, CancellationToken cancellationToken = default)
        {
            TEntity item = null;
            try
            {
                item = SetQuery(null).SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)?.Result ?? new TEntity();
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
                list = await SetQuery(null).Map<TEntity, TResponse>(ServiceConfig).ToListAsync(cancellationToken);
                GetFiles(list);
                return Found(list);
            }
            catch (Exception exception)
            {
                return Error(exception, list);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse(PageOrderRequest request, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                if (ServiceConfig.PageOrder)
                {
                    var order = new Order() { Expression = request.OrderExpression ?? string.Empty };
                    if (request.PageOrderSession && Settings.SessionExpirationInMinutes > 0)
                    {
                        var pageFromSession = GetSession<Page>(nameof(Page));
                        if (pageFromSession is not null)
                        {
                            request.Page.Number = pageFromSession.Number;
                            request.Page.RecordsPerPageCount = pageFromSession.RecordsPerPageCount;
                        }
                        var orderFromSession = GetSession<Order>(nameof(Order));
                        if (orderFromSession is not null)
                        {
                            order.Expression = orderFromSession.Expression;
                        }
                    }
                    request.Page.RecordsPerPageCounts = ServiceConfig.RecordsPerPageCounts;
                    order.Expressions = ServiceConfig.OrderExpressions;
                    list = await SetQuery(null).OrderBy(order.Expression).Paginate(request.Page).Map<TEntity, TResponse>(ServiceConfig).ToListAsync(cancellationToken);
                    if (Settings.SessionExpirationInMinutes > 0)
                    {
                        CreateSession(nameof(Page), request.Page);
                        CreateSession(nameof(Order), order);
                    }
                    GetFiles(list);
                    return Found(list, request.Page, order);
                }
                return await GetResponse(cancellationToken);
            }
            catch (Exception exception)
            {
                return Error(exception, list);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetResponse(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await SetQuery(null).Where(predicate).Map<TEntity, TResponse>(ServiceConfig).ToListAsync(cancellationToken);
                GetFiles(list);
                return Found(list);
            }
            catch (Exception exception)
            {
                return Error(exception, list);
            }
        }

        public virtual async Task<Result<TResponse>> GetResponse(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await SetQuery(null).Map<TEntity, TResponse>(ServiceConfig).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                GetFiles(item);
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(exception, item);
            }
        }

        public virtual async Task<Result<TRequest>> GetRequest(int? id = default, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                if (id.HasValue)
                {
                    var entity = await SetQuery(null).SingleOrDefaultAsync(entity => entity.Id == id.Value, cancellationToken);
                    item = entity.Map(ServiceConfig, item);
                    GetFiles(item, entity);
                }
                else
                {
                    item = new TRequest();
                }
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(exception, item);
            }
        }

        public virtual async Task<Result<TRequest>> GetRequest(string guid, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                var entity = await SetQuery(null).SingleOrDefaultAsync(entity => entity.Guid == guid, cancellationToken);
                item = entity.Map(ServiceConfig, item);
                GetFiles(item, entity);
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(exception, item);
            }
        }

        public Result<T> GetRequest<T>() where T : Request, new()
        {
            var item = new T();
            return Success(item);
        }

        protected virtual Result<TRequest> Validate(TRequest request)
        {
            try
            {
                Property entityProperty, requestProperty;
                string collation = "Turkish_CI_AS";
                string modelStateErrors = Validate(request.ModelState).Message;
                string uniquePropertyError = string.Empty;
                if (ServiceConfig.SqlServer)
                {
                    foreach (var uniqueProperty in ServiceConfig.UniqueProperties)
                    {
                        entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                        requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                        if (entityProperty is not null && requestProperty is not null && SetQuery(null).Any(entity => entity.Id != request.Id &&
                            EF.Functions.Collate(EF.Property<string>(entity, entityProperty.Name), collation) == EF.Functions.Collate((requestProperty.Value ?? "").ToString(), collation).Trim()))
                        {
                            if (Culture == Defaults.TR)
                                uniquePropertyError = $"{requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} değerine sahip {ServiceConfig.Title.ToLower()} bulunmaktadır!";
                            else
                                uniquePropertyError = $"{ServiceConfig.Title} with the same value for {requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} exists!";
                            break;
                        }
                    }
                }
                return Validated(request, modelStateErrors, uniquePropertyError);
            }
            catch (Exception exception)
            {
                return Error(exception, request);
            }
        }

        protected void Validate<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null)
                RelationsFound = ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)) && relationalEntities.Any();
        }

        protected void Update<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
                Db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
        }

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
            {
                if (!ServiceConfig.SoftDelete)
                    Db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
            }
        }

        public virtual async Task<Result<TRequest>> Create(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return Result(validationResult, request);
                var entity = request.Map<TRequest, TEntity>(ServiceConfig).Trim();
                entity.Guid = Guid.NewGuid().ToString();
                entity.CreateDate = DateTime.Now;
                entity.CreatedBy = GetUserName() ?? Defaults.User;
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
                return Error(exception, request);
            }
        }

        public virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return Result(validationResult, request);
                var entity = Db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return NotFound(request);
                var guid = entity.Guid;
                var createDate = entity.CreateDate;
                var createdBy = entity.CreatedBy;
                entity = request.Map(ServiceConfig, entity).Trim();
                entity.Guid = guid;
                entity.CreateDate = createDate;
                entity.CreatedBy = createdBy;
                entity.UpdateDate = DateTime.Now;
                entity.UpdatedBy = GetUserName();
                var fileResult = UpdateFiles(request, entity);
                if (!fileResult.Success)
                    return Result(fileResult, request);
                Db.Set<TEntity>().Update(entity);
                if (save)
                {
                    try
                    {
                        await Save(cancellationToken);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return NotFound(request);
                    }
                }
                return Updated(request);
            }
            catch (Exception exception)
            {
                return Error(exception, request);
            }
        }

        public virtual async Task<Result<TRequest>> Delete(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (RelationsFound)
                    return RelationsFound(request);
                var entity = Db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return NotFound(request);
                if (ServiceConfig.SoftDelete)
                {
                    entity.Deleted = true;
                    entity.UpdateDate = DateTime.Now;
                    entity.UpdatedBy = GetUserName();
                    Db.Set<TEntity>().Update(entity);
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
                return Error(exception, request);
            }
        }

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
                        return NotFound(request);
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
                            if (fileEntity.OtherFiles is not null && fileEntity.OtherFiles.Any())
                            {
                                var lastOtherFile = fileEntity.OtherFiles.Order().Last();
                                orderInitialValue = GetFileOrder(lastOtherFile) + 1;
                            }
                            var otherFilesResult = CreateFiles(fileRequest.OtherFormFiles);
                            if (otherFilesResult.Success)
                            {
                                if (otherFilesResult.Data is not null && otherFilesResult.Data.Any())
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
                if (string.IsNullOrWhiteSpace(filePath))
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
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        fileEntity.OtherFiles.Remove(filePath);
                        if (!fileEntity.OtherFiles.Any())
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
                return NotFound(request);
            var deleteResult = DeleteFiles(request, entity, filePath);
            if (!deleteResult.Success)
                return Result(deleteResult, request);
            await Save(cancellationToken);
            return Success(request);
        }

        protected void GetFiles(TResponse item)
        {
            if (item is FileResponse && item is not null)
                GetOtherFilePaths((item as FileResponse).OtherFiles);
        }

        protected void GetFiles(List<TResponse> list)
        {
            if (list.Any())
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
