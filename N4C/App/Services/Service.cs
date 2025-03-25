using Microsoft.EntityFrameworkCore;
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
    public abstract class Service<TEntity, TRequest, TResponse> : Service, IDisposable
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        private PropertyInfo GuidProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(Entity.Guid));
        private PropertyInfo DeletedProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IDeleted.Deleted));
        private PropertyInfo CreateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreateDate));
        private PropertyInfo CreatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreatedBy));
        private PropertyInfo UpdateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdateDate));
        private PropertyInfo UpdatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdatedBy));
        private PropertyInfo MainFileProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(FileEntity.MainFile));
        private PropertyInfo OtherFilesProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(FileEntity.OtherFiles));

        private bool HasGuid => GuidProperty is not null;
        private bool HasDeleted => DeletedProperty is not null;
        private bool HasModified => CreateDateProperty is not null && CreatedByProperty is not null && UpdateDateProperty is not null && UpdatedByProperty is not null;
        private bool HasFile => MainFileProperty is not null && OtherFilesProperty is not null;

        private bool RelationalEntitiesFound { get; set; }
        private List<string> UniqueProperties { get; set; }
        private bool PageOrder { get; set; }
        private List<string> RecordsPerPageCounts { get; set; } = new List<string>();
        private Dictionary<string, string> OrderExpressions { get; set; } = new Dictionary<string, string>();

        protected string Collation { get; private set; }
        protected string RecordNotSaved { get; private set; }
        protected string RecordSaved { get; private set; }
        protected string RecordHasRelationalRecords { get; private set; }
        protected MapperProfile MapperProfile { get; private set; } = new MapperProfile();
        
        protected string TrueHtml { get; private set; }
        protected string FalseHtml { get; private set; }

        public string Title { get; private set; }

        private readonly IDb _db;

        protected HttpService HttpService { get; }
        protected FileService FileService { get; }

        protected Service(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(logService)
        {
            _db = db;
            HttpService = httpService;
            FileService = fileService;
            SetCulture(HttpService.Culture);
            FileService.SetCulture(Culture);
            SetCollation("Turkish_CI_AS");
            SetRecordsPerPageCounts(5, 10, 25, 50, 100);
            SetTrueHtml("<i class='bx bx-check fs-3'></i>");
            SetFalseHtml("<i class='bx bx-x fs-3'></i>");
            UniqueProperties = ["Name", "UserName", "Title"];
        }

        internal override void SetCulture(string culture)
        {
            base.SetCulture(culture);
            SetTitle(Culture == Cultures.TR ? "Kayıt" : "Record");
            SetRecordsPerPageCounts(5, 10, 25, 50, 100);
            RecordNotSaved = Culture == Cultures.TR ? "Kaydedilmedi" : "Not saved";
            RecordSaved = Culture == Cultures.TR ? "Kaydedildi. Etkilenen satır sayısı:" : "Saved. Number of rows effected:";
            RecordHasRelationalRecords = Culture == Cultures.TR ? "İlişkili kayıtlar bulunmaktadır" : "Relational records found";
        }

        protected void SetTitle(string tr, string en = default)
        {
            Title = tr;
            if (Culture == Cultures.EN)
            {
                Title = typeof(TEntity).Name;
                if (!string.IsNullOrWhiteSpace(en))
                    Title = en;
            }
            NotFound = Culture == Cultures.TR ? $"{Title} bulunamadı" : $"{Title} record(s) not found";
            Found = Culture == Cultures.TR ? $"{Title.ToLower()} bulundu" : $"{Title.ToLower()} record(s) found";
            Created = Culture == Cultures.TR ? $"{Title} başarıyla oluşturuldu" : $"{Title} created successfully";
            Updated = Culture == Cultures.TR ? $"{Title} başarıyla güncellendi" : $"{Title} updated successfully";
            Deleted = Culture == Cultures.TR ? $"{Title} başarıyla silindi" : $"{Title} deleted successfully";
        }

        protected void SetCollation(string collation)
        {
            Collation = collation;
        }

        protected void SetUniqueProperties(params Expression<Func<TRequest, object>>[] uniqueProperties)
        {
            UniqueProperties.Clear();
            foreach (var uniqueProperty in uniqueProperties)
            {
                UniqueProperties.Add(uniqueProperty.GetProperty().Name);
            }
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
            RecordsPerPageCounts.Clear();
            if (recordsPerPageCounts.Any())
            {
                foreach (var recordsPerPageCount in recordsPerPageCounts)
                {
                    RecordsPerPageCounts.Add(recordsPerPageCount.ToString());
                }
                RecordsPerPageCounts.Add(Culture == Cultures.TR ? "Tümü" : "All");
            }
        }

        protected void SetOrderExpressions(params Expression<Func<TResponse, object>>[] entityProperties)
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

        protected TEntity Query(TRequest request)
        {
            if (request.Guid.IsGuid())
                return Query(request.Guid);
            return Query(request.Id);
        }

        public virtual async Task<Result<List<TResponse>>> GetList(CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Query().Map<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                if (list.Count > 0)
                {
                    if (HasFile)
                    {
                        foreach (var item in list)
                        {
                            if (item is FileResponse)
                                FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                        }
                    }
                    return Success(list, $"{list.Count} {Found}");
                }
                return Error(list, HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetList(): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetList(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Query().Where(predicate).Map<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                if (list.Count > 0)
                {
                    if (HasFile)
                    {
                        foreach (var item in list)
                        {
                            if (item is FileResponse)
                                FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                        }
                    }
                    return Success(list, $"{list.Count} {Found}", HttpStatusCode.PartialContent);
                }
                return Error(list, HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetList(predicate): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<List<TResponse>>> GetList(PageOrder pageOrder, CancellationToken cancellationToken = default)
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
                    list = await Query().OrderBy(pageOrder).Paginate(pageOrder).Map<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                    if (Settings.SessionExpirationInMinutes > 0)
                        HttpService.SetSession(nameof(PageOrder), pageOrder);
                    if (pageOrder.TotalRecordsCount > 0)
                    {
                        if (HasFile)
                        {
                            foreach (var item in list)
                            {
                                if (item is FileResponse)
                                    FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                            }
                        }
                        return Success(list, $"{pageOrder.TotalRecordsCount} {Found}");
                    }
                    return Error(list, HttpStatusCode.NotFound);
                }
                return await GetList(cancellationToken);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetList(pageOrder): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TResponse>> GetItem(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await Query().Map<TEntity, TResponse>(MapperProfile).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                if (item is null)
                    return Error(item, HttpStatusCode.NotFound);
                if (HasFile && item is FileResponse)
                {
                    FileService.GetOtherFiles((item as FileResponse).OtherFiles);
                }
                return Success(item, $"1 {Found}", HttpStatusCode.PartialContent);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetItem(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        protected virtual Result<TRequest> Validate(TRequest request)
        {
            try
            {
                Property entityProperty, requestProperty;
                string error = string.Empty;
                List<string> errors = request.ModelState.GetErrors(Culture);
                foreach (var uniqueProperty in UniqueProperties)
                {
                    entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                    requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                    if (entityProperty is not null && requestProperty is not null && Query().Any(entity => entity.Id != request.Id &&
                        EF.Functions.Collate(EF.Property<string>(entity, entityProperty.Name), Collation) == EF.Functions.Collate((requestProperty.Value ?? "").ToString(), Collation).Trim()))
                    {
                        error = $"{requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} değerine sahip {Title.ToLower()} bulunmaktadır!";
                        if (Culture == Cultures.EN)
                            error = $"{Title} with the same value for {requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} exists!";
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(error))
                    errors.Add(error);
                if (errors.Any())
                    return Error(request, string.Join(";", errors));
                return Success(request);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.Validate(Id = {request.Id}): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
            }
        }

        protected void Validate<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null)
                RelationalEntitiesFound = ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)) && relationalEntities.Any();
        }

        protected void Update<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
                _db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
        }

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new()
        {
            if (relationalEntities is not null && ObjectExtensions.GetPropertyInfo<TRelationalEntity>().Any(property => property.PropertyType == typeof(TEntity)))
            {
                if (!HasDeleted)
                    _db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
            }
        }

        public Result<TRequest> GetItemForCreate()
        {
            return Success(new TRequest());
        }

        public virtual async Task<Result<TRequest>> Create(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return validationResult;
                var entity = request.Map<TRequest, TEntity>(MapperProfile).Trim();
                var fileResult = CreateFiles(request, entity);
                if (!fileResult.Success)
                    return fileResult;
                if (HasGuid)
                {
                    entity.Guid = Guid.NewGuid().ToString();
                }
                if (HasModified)
                {
                    (entity as IModified).CreateDate = DateTime.Now;
                    (entity as IModified).CreatedBy = HttpService.GetUserName();
                }
                _db.Set<TEntity>().Add(entity);
                if (save)
                {
                    var result = await Save(cancellationToken);
                    if (result.Success)
                    {
                        request.Id = entity.Id;
                        request.Guid = entity.Guid;
                        return Success(request, Created, HttpStatusCode.Created);
                    }
                    return Error(request, result);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.Create(): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
            }
        }

        public Result<TRequest> GetItemForEdit(int id)
        {
            TRequest item = null;
            try
            {
                var entity = Query(id);
                if (entity is null)
                    return Error(item, HttpStatusCode.NotFound);
                item = entity.Map<TEntity, TRequest>(MapperProfile);
                if (HasFile && item is FileRequest)
                    (item as FileRequest).MainFile = (entity as FileEntity).MainFile;
                return Success(item);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetItemForEdit(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public Result<TRequest> GetItemForEdit(string guid)
        {
            TRequest item = null;
            try
            {
                var entity = Query(guid);
                if (entity is null)
                    return Error(item, HttpStatusCode.NotFound);
                item = entity.Map<TEntity, TRequest>(MapperProfile);
                if (HasFile && item is FileRequest)
                    (item as FileRequest).MainFile = (entity as FileEntity).MainFile;
                return Success(item);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetItemForEdit(Guid = {guid}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return validationResult;
                var entity = _db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return Error(request, HttpStatusCode.NotFound);
                entity = request.Map(MapperProfile, entity).Trim();
                var fileResult = UpdateFiles(request, entity);
                if (!fileResult.Success)
                    return fileResult;
                if (HasModified)
                {
                    (entity as IModified).UpdateDate = DateTime.Now;
                    (entity as IModified).UpdatedBy = HttpService.GetUserName();
                }
                _db.Set<TEntity>().Update(entity);
                if (save)
                {
                    try
                    {
                        var result = await Save(cancellationToken);
                        if (result.Success)
                            return Success(request, Updated, HttpStatusCode.NoContent);
                        return Error(request, result);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return Error(request, HttpStatusCode.NotFound);
                    }
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.Update(Id = {request.Id}): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
            }
        }

        public Result<TResponse> GetItemForDelete(int id)
        {
            TResponse item = null;
            try
            {
                return GetItem(id).Result;
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.GetItemForDelete(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TRequest>> Delete(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (RelationalEntitiesFound)
                    return Error(request, RecordHasRelationalRecords);
                var entity = _db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return Error(request, HttpStatusCode.NotFound);
                if (HasDeleted)
                {
                    (entity as IDeleted).Deleted = true;
                    if (HasModified)
                    {
                        (entity as IModified).UpdateDate = DateTime.Now;
                        (entity as IModified).UpdatedBy = HttpService.GetUserName();
                    }
                    _db.Set<TEntity>().Update(entity);
                }
                else
                {
                    var fileResult = DeleteFiles(request, entity);
                    if (!fileResult.Success)
                        return fileResult;
                    _db.Set<TEntity>().Remove(entity);
                }
                if (save)
                {
                    var result = await Save(cancellationToken);
                    if (result.Success)
                        return Success(request, Deleted, HttpStatusCode.NoContent);
                    return Error(request, result);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.Delete(Id = {request.Id}): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
            }
        }

        protected async Task<Result> Save(CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var entityEntry in _db.ChangeTracker.Entries<TEntity>())
                {
                    switch (entityEntry.State)
                    {
                        case EntityState.Modified:
                            if (HasGuid)
                                entityEntry.Property(GuidProperty.Name).IsModified = false;
                            if (HasModified)
                            {
                                entityEntry.Property(CreateDateProperty.Name).IsModified = false;
                                entityEntry.Property(CreatedByProperty.Name).IsModified = false;
                            }
                            break;
                        case EntityState.Deleted:
                            if (HasGuid && HasDeleted)
                                entityEntry.Property(GuidProperty.Name).IsModified = false;
                            break;
                    }
                }
                var numberOfStateEntries = await _db.SaveChangesAsync(cancellationToken);
                return Success($"{RecordSaved} {numberOfStateEntries}");
            }
            catch (Exception exception)
            {
                LogService.LogError($"ServiceException: {GetType().Name}.Save(): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        protected Result<TRequest> CreateFiles(TRequest request, TEntity entity)
        {
            if (HasFile && request is FileRequest)
            {
                var fileRequest = request as FileRequest;
                var validationResult = FileService.ValidateOtherFiles(fileRequest?.OtherFormFiles);
                if (validationResult.Success)
                {
                    var mainFileResult = FileService.CreateFile(fileRequest.MainFormFile);
                    if (mainFileResult.Success)
                    {
                        var fileEntity = entity as FileEntity;
                        fileEntity.MainFile = mainFileResult.Data.MainFile;
                        var otherFilesResult = FileService.CreateFiles(fileRequest.OtherFormFiles);
                        if (otherFilesResult.Success)
                            fileEntity.OtherFiles = FileService.GetOtherFiles(otherFilesResult.Data, 1);
                        else
                            return Error(request, otherFilesResult);
                    }
                    else
                    {
                        return Error(request, mainFileResult);
                    }
                }
            }
            return Success(request);
        }

        protected Result<TRequest> UpdateFiles(TRequest request, TEntity entity = null)
        {
            if (HasFile && request is FileRequest)
            {
                if (entity is null)
                    entity = _db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                {
                    return Error(request, HttpStatusCode.NotFound);
                }
                else
                {
                    var fileRequest = request as FileRequest;
                    var fileEntity = entity as FileEntity;
                    var validationResult = FileService.ValidateOtherFiles(fileRequest?.OtherFormFiles, fileEntity.OtherFiles);
                    if (validationResult.Success)
                    {
                        var mainFileResult = FileService.UpdateFile(fileRequest.MainFormFile, fileEntity.MainFile);
                        if (mainFileResult.Success)
                        {
                            fileEntity.MainFile = mainFileResult.Data.MainFile;
                            var orderInitialValue = 1;
                            if (fileEntity.OtherFiles is not null && fileEntity.OtherFiles.Any())
                            {
                                var lastOtherFile = fileEntity.OtherFiles.Order().Last();
                                orderInitialValue = FileService.GetFileOrder(lastOtherFile) + 1;
                            }
                            var otherFilesResult = FileService.CreateFiles(fileRequest.OtherFormFiles);
                            if (otherFilesResult.Success)
                            {
                                if (otherFilesResult.Data is not null && otherFilesResult.Data.Any())
                                    fileEntity.OtherFiles = FileService.GetOtherFiles(otherFilesResult.Data, orderInitialValue);
                            }
                            else
                            {
                                return Error(request, otherFilesResult);
                            }
                        }
                        else
                        {
                            return Error(request, mainFileResult);
                        }
                    }
                }
            }
            return Success(request);
        }

        protected Result<TRequest> DeleteFiles(TRequest request, TEntity entity, string filePath = null)
        {
            Result<FileResponse> mainFileResult;
            Result<List<FileResponse>> otherFilesResult;
            if (HasFile && request is FileRequest)
            {
                var fileEntity = entity as FileEntity;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    mainFileResult = FileService.DeleteFile(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult);
                    fileEntity.MainFile = null;
                    otherFilesResult = FileService.DeleteFiles(fileEntity.OtherFiles);
                    if (!otherFilesResult.Success)
                        return Error(request, otherFilesResult);
                    fileEntity.OtherFiles = null;
                }
                else if (filePath == fileEntity.MainFile)
                {
                    mainFileResult = FileService.DeleteFile(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult);
                    fileEntity.MainFile = null;
                }
                else
                {
                    mainFileResult = FileService.DeleteFile(filePath);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult);
                    filePath = fileEntity.OtherFiles.SingleOrDefault(otherFile =>
                        $"/{FileService.GetFileFolder(otherFile)}/{FileService.GetFileName(otherFile)}" == filePath);
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        fileEntity.OtherFiles.Remove(filePath);
                        if (!fileEntity.OtherFiles.Any())
                            fileEntity.OtherFiles = null;
                    }
                }
                _db.Set<TEntity>().Update(entity);
            }
            return Success(request);
        }

        public async Task<Result<TRequest>> DeleteFiles(int id, string filePath = null, CancellationToken cancellationToken = default)
        {
            var request = new TRequest() { Id = id };
            var entity = _db.Set<TEntity>().Find(request.Id);
            if (entity is null)
            {
                return Error(request, HttpStatusCode.NotFound);
            }
            else
            {
                var deleteResult = DeleteFiles(request, entity, filePath);
                if (!deleteResult.Success)
                    return Error(request, deleteResult);
                var saveResult = await Save(cancellationToken);
                if (!saveResult.Success)
                    return Error(request, saveResult);
            }
            return Success(request);
        }

        public Result<FileResponse> GetFile(string filePath)
        {
            return FileService.GetFile(filePath);
        }

        public async Task GetExcel()
        {
            var result = await GetList();
            if (result.Success)
                FileService.GetExcel(result.Data);
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
