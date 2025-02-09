using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App.Services;
using N4C.Domain;
using N4C.Extensions;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace N4C.App
{
    public class Application
    {
        public string Failed { get; private set; }
        public string Successful { get; private set; }
        public string Exception { get; private set; }

        protected string NotFound { get; set; }

        private string _culture;
        public string Culture
        {
            get => _culture;
            private set
            {
                _culture = value;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(_culture);
                Failed = _culture == Cultures.TR ? "İşlem gerçekleştirilemedi!" : "Operation failed!";
                Successful = _culture == Cultures.TR ? "İşlem başarıyla gerçekleştirildi." : "Operation successful.";
                Exception = _culture == Cultures.TR ? "İşlem sırasında hata meydana geldi!" : "An exception occurred during the operation!";
                NotFound = _culture == Cultures.TR ? "Kayıt bulunamadı!" : "Record not found!";
            }
        }

        protected HttpService HttpService { get; }
        protected ILogger<Application> Logger { get; }

        public Application(HttpService httpService, ILogger<Application> logger)
        {
            HttpService = httpService;
            Logger = logger;
            Culture = HttpService.GetCookie(nameof(Culture)) ?? Settings.Culture;
        }

        protected void SetCulture(string culture)
        {
            Culture = culture;
        }

        public Result Success()
        {
            return new Result(HttpStatusCode.OK);
        }

        public Result Success(string message)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result(HttpStatusCode.OK, message);
        }

        public Result Error(HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            return new Result(httpStatusCode,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : Failed);
        }

        public Result Error(string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result(HttpStatusCode.BadRequest, message);
        }

        public Result<TData> Success<TData>(TData data)
        {
            return new Result<TData>(HttpStatusCode.OK, data);
        }

        public Result<TData> Success<TData>(TData data, string message)
        {
            if (!message.EndsWith("."))
                message += ".";
            return new Result<TData>(HttpStatusCode.OK, data, message);
        }

        public Result<TData> Error<TData>(TData data, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : Failed);
        }

        public Result<TData> Error<TData>(TData data, string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(HttpStatusCode.BadRequest, data, message);
        }

        public Result<TData> Error<TData>(TData data, HttpStatusCode httpStatusCode, string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(httpStatusCode, data,
                httpStatusCode == HttpStatusCode.InternalServerError ? Exception :
                httpStatusCode == HttpStatusCode.NotFound ? NotFound : message);
        }
    }

    public abstract class Application<TEntity, TRequest, TResponse> : Application, IDisposable
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        private PropertyInfo GuidProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(Entity.Guid));
        private PropertyInfo DeletedProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.Deleted));
        private PropertyInfo CreateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreateDate));
        private PropertyInfo CreatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreatedBy));
        private PropertyInfo UpdateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdateDate));
        private PropertyInfo UpdatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdatedBy));
        private PropertyInfo MainFileProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IFile.MainFile));
        private PropertyInfo OtherFilesProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IFile.OtherFiles));

        protected bool HasFile => MainFileProperty is not null && OtherFilesProperty is not null;

        private bool HasGuid => GuidProperty is not null;
        private bool HasDeleted => DeletedProperty is not null;
        private bool HasModified => CreateDateProperty is not null && CreatedByProperty is not null &&
            UpdateDateProperty is not null && UpdatedByProperty is not null;

        private bool RelationalEntitiesFound { get; set; }
        private List<string> UniqueProperties { get; set; }

        protected MapperProfile MapperProfile { get; private set; }
        protected string Collation { get; private set; }
        protected string RecordNotSaved { get; private set; }
        protected string RecordSaved { get; private set; }
        protected string RecordHasRelationalRecords { get; private set; }
        protected string RecordsFound { get; private set; }
        protected string RecordCreated { get; private set; }
        protected string RecordUpdated { get; private set; }
        protected string RecordDeleted { get; private set; }

        private string _title;
        public string Title
        {
            get => _title;
            private set
            {
                _title = value;
                RecordsFound = Culture == Cultures.TR ? $"{_title.ToLower()} bulundu" : $"{_title.ToLower()} record(s) found";
                RecordCreated = Culture == Cultures.TR ? $"{_title} başarıyla oluşturuldu" : $"{_title} created successfully";
                RecordUpdated = Culture == Cultures.TR ? $"{_title} başarıyla güncellendi" : $"{_title} updated successfully";
                RecordDeleted = Culture == Cultures.TR ? $"{_title} başarıyla silindi" : $"{_title} deleted successfully";
            }
        }

        protected FileService FileService { get; set; }

        private readonly IDb _db;

        protected Application(IDb db, HttpService httpService, ILogger<Application> logger) : base(httpService, logger)
        {
            _db = db;
            UniqueProperties = ["Name", "UserName", "Title"];
            MapperProfile = new MapperProfile();
            Collation = "Turkish_CI_AS";
            Title = Culture == Cultures.TR ? "Kayıt" : "Record";
            RecordNotSaved = Culture == Cultures.TR ? "Kaydedilmedi" : "Not saved";
            RecordSaved = Culture == Cultures.TR ? "Kaydedildi. Etkilenen satır sayısı:" : "Saved. Number of rows effected:";
            RecordHasRelationalRecords = Culture == Cultures.TR ? "İlişkili kayıtlar bulunmaktadır" : "Relational records found";
        }

        protected void SetCollation(string collation)
        {
            Collation = collation;
        }

        protected void SetTitle(string tr, string en = default)
        {
            Title = !string.IsNullOrWhiteSpace(en) && Culture == Cultures.EN ? en : tr;
        }

        protected void SetUniqueProperties(params Expression<Func<TRequest, object>>[] uniqueProperties)
        {
            Property property;
            foreach (var uniqueProperty in uniqueProperties)
            {
                property = uniqueProperty.GetProperty();
                if (!UniqueProperties.Contains(property.Name))
                    UniqueProperties.Add(property.Name);
            }
        }

        protected virtual IQueryable<TEntity> Data(Action<MapperProfile> mapperProfile = default)
        {
            if (mapperProfile is not null)
                mapperProfile.Invoke(MapperProfile);
            var query = _db.Set<TEntity>().AsNoTracking();
            if (HasDeleted)
                query = query.Where(entity => EF.Property<bool>(entity, DeletedProperty.Name) == false);
            return query;
        }

        protected TEntity Data(TRequest request)
        {
            try
            {
                return Data().SingleOrDefault(entity => entity.Id == request.Id) ?? new TEntity();
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.Data(Request.Id = {request.Id}): {exception.Message}");
                return new TEntity();
            }
        }

        public async Task<Result<List<TResponse>>> GetList(CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Data().ProjectTo<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                if (list.Count > 0)
                {
                    if (HasFile)
                    {
                        FileService = new FileService(HttpService, Logger);
                        foreach (var item in list)
                        {
                            if (item is IFileResponse)
                                FileService.UpdateOtherFiles((item as IFileResponse).OtherFiles);
                        }
                    }
                    return Success(list, $"{list.Count} {RecordsFound}");
                }
                return Error(list, HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.GetList(): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<TResponse>> GetItem(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await Data().ProjectTo<TEntity, TResponse>(MapperProfile).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                if (item is null)
                    return Error(item, HttpStatusCode.NotFound);
                if (HasFile && item is IFileResponse)
                {
                    FileService = new FileService(HttpService, Logger);
                    FileService.UpdateOtherFiles((item as IFileResponse).OtherFiles);
                }
                return Success(item, $"1 {RecordsFound}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.GetItem(Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual Result<TRequest> Validate(TRequest request, ModelStateDictionary modelState = default, string errorMessagesSeperator = "; ")
        {
            try
            {
                List<string> errorMessages = new List<string>();
                if (modelState is not null)
                    errorMessages.AddRange(modelState.GetErrors(Culture));
                Property entityProperty, requestProperty;
                string errorMessage = string.Empty;
                foreach (var uniqueProperty in UniqueProperties)
                {
                    entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                    requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                    if (entityProperty is not null && requestProperty is not null && Data().Any(entity => entity.Id != request.Id &&
                        EF.Functions.Collate(EF.Property<string>(entity, entityProperty.Name), Collation) == EF.Functions.Collate((requestProperty.Value ?? "").ToString(), Collation).Trim()))
                    {
                        errorMessage = $"{requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} değerine sahip {Title.ToLower()} bulunmaktadır!";
                        if (Culture == Cultures.EN)
                            errorMessage = $"{Title} with the same value for {requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} exists!";
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    errorMessages.Add(errorMessage);
                if (errorMessages.Any())
                {
                    if (errorMessagesSeperator == "<br>" || errorMessagesSeperator == "<br />" || errorMessagesSeperator == "<br/>")
                        errorMessages.Insert(0, string.Empty);
                    return Error(request, string.Join(errorMessagesSeperator, errorMessages));
                }
                return Success(request);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.Validate(Request.Id = {request.Id}): {exception.Message}");
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

        public async Task<Result<TRequest>> Create(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
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
                    (entity as IModified).CreatedBy = HttpService.UserName;
                }
                _db.Set<TEntity>().Add(entity);
                if (save)
                {
                    var result = await Save(cancellationToken);
                    if (result.Success)
                    {
                        request.Id = entity.Id;
                        request.Guid = entity.Guid;
                        return Success(request, RecordCreated);
                    }
                    return Error(request, result.HttpStatusCode);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.Create(Request.Id = {request.Id}): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
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
                    (entity as IModified).UpdatedBy = HttpService.UserName;
                }
                _db.Set<TEntity>().Update(entity);
                if (save)
                {
                    try
                    {
                        var result = await Save(cancellationToken);
                        if (result.Success)
                            return Success(request, RecordUpdated);
                        return Error(request, result.HttpStatusCode);
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
                Logger.LogError($"ApplicationException: {GetType().Name}.Update(Request.Id = {request.Id}): {exception.Message}");
                return Error(request, HttpStatusCode.InternalServerError);
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
                    (entity as IModified).Deleted = true;
                    if (HasModified)
                    {
                        (entity as IModified).UpdateDate = DateTime.Now;
                        (entity as IModified).UpdatedBy = HttpService.UserName;
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
                        return Success(request, RecordDeleted);
                    return Error(request, result.HttpStatusCode);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.Delete(Request.Id = {request.Id}): {exception.Message}");
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
                Logger.LogError($"ApplicationException: {GetType().Name}.Save(): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        protected Result<TRequest> CreateFiles(TRequest request, TEntity entity)
        {
            if (HasFile && request is IFileRequest)
            {
                FileService = new FileService(HttpService, Logger);
                var fileRequest = request as IFileRequest;
                var validationResult = FileService.ValidateOtherFiles(fileRequest?.OtherFormFiles);
                if (validationResult.Success)
                {
                    var mainFileResult = FileService.Create(fileRequest.MainFormFile);
                    if (mainFileResult.Success)
                    {
                        var fileEntity = entity as IFile;
                        fileEntity.MainFile = mainFileResult.Data.MainFile;
                        var otherFilesResult = FileService.Create(fileRequest.OtherFormFiles);
                        if (otherFilesResult.Success)
                            fileEntity.OtherFiles = FileService.GetOtherFiles(otherFilesResult.Data, 1);
                        else
                            return Error(request, otherFilesResult.HttpStatusCode, otherFilesResult.Message);
                    }
                    else
                    {
                        return Error(request, mainFileResult.HttpStatusCode, mainFileResult.Message);
                    }
                }
            }
            return Success(request);
        }

        protected Result<TRequest> UpdateFiles(TRequest request, TEntity entity = null)
        {
            if (HasFile)
            {
                FileService = new FileService(HttpService, Logger);
                if (entity is null)
                    entity = _db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                {
                    return Error(request, NotFound);
                }
                else
                {
                    var fileRequest = request as IFileRequest;
                    var fileEntity = entity as IFile;
                    var validationResult = FileService.ValidateOtherFiles(fileRequest?.OtherFormFiles, fileEntity.OtherFiles);
                    if (validationResult.Success)
                    {
                        var mainFileResult = FileService.Update(fileRequest.MainFormFile, fileEntity.MainFile);
                        if (mainFileResult.Success)
                        {
                            fileEntity.MainFile = mainFileResult.Data.MainFile;
                            var orderInitialValue = 1;
                            if (fileEntity.OtherFiles is not null && fileEntity.OtherFiles.Any())
                            {
                                var lastOtherFile = fileEntity.OtherFiles.Order().Last();
                                orderInitialValue = FileService.GetFileOrder(lastOtherFile) + 1;
                            }
                            var otherFilesResult = FileService.Create(fileRequest.OtherFormFiles);
                            if (otherFilesResult.Success)
                            {
                                if (otherFilesResult.Data is not null && otherFilesResult.Data.Any())
                                    fileEntity.OtherFiles = FileService.GetOtherFiles(otherFilesResult.Data, orderInitialValue);
                            }
                            else
                            {
                                return Error(request, otherFilesResult.HttpStatusCode, otherFilesResult.Message);
                            }
                        }
                        else
                        {
                            return Error(request, mainFileResult.HttpStatusCode, mainFileResult.Message);
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
            if (HasFile)
            {
                FileService = new FileService(HttpService, Logger);
                var fileEntity = entity as IFile;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    mainFileResult = FileService.Delete(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult.HttpStatusCode, mainFileResult.Message);
                    fileEntity.MainFile = null;
                    otherFilesResult = FileService.Delete(fileEntity.OtherFiles);
                    if (!otherFilesResult.Success)
                        return Error(request, otherFilesResult.HttpStatusCode, otherFilesResult.Message);
                    fileEntity.OtherFiles = null;
                }
                else if (filePath == fileEntity.MainFile)
                {
                    mainFileResult = FileService.Delete(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult.HttpStatusCode, mainFileResult.Message);
                    fileEntity.MainFile = null;
                }
                else
                {
                    mainFileResult = FileService.Delete(filePath);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult.HttpStatusCode, mainFileResult.Message);
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
                return Error(request, NotFound);
            }
            else
            {
                var deleteResult = DeleteFiles(request, entity, filePath);
                if (!deleteResult.Success)
                    return Error(request, deleteResult.HttpStatusCode, deleteResult.Message);
                var saveResult = await Save(cancellationToken);
                if (!saveResult.Success)
                    return Error(request, saveResult.HttpStatusCode, saveResult.Message);
            }
            return Success(request);
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
