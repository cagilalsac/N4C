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

        protected string RecordNotFound { get; private set; }

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
                RecordNotFound = _culture == Cultures.TR ? "Kayıt bulunamadı!" : "Record not found!";
            }
        }

        protected HttpService HttpService { get; }

        public Application(HttpService httpService)
        {
            HttpService = httpService;
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
                httpStatusCode == HttpStatusCode.InternalServerError ? $"{Exception}" :
                httpStatusCode == HttpStatusCode.NotFound ? $"{RecordNotFound}" : $"{Failed}");
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
                httpStatusCode == HttpStatusCode.InternalServerError ? $"{Exception}" :
                httpStatusCode == HttpStatusCode.NotFound ? $"{RecordNotFound}" : $"{Failed}");
        }

        public Result<TData> Error<TData>(TData data, string message)
        {
            if (!message.StartsWith(Failed))
                message = $"{Failed} {message}";
            if (!message.EndsWith("!"))
                message += "!";
            return new Result<TData>(HttpStatusCode.BadRequest, data, message);
        }
    }

    public abstract class Application<TEntity, TRequest, TResponse> : Application, IDisposable
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        private PropertyInfo EntityGuidProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(Entity.Guid));
        private PropertyInfo EntityDeletedProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.Deleted));
        private PropertyInfo EntityCreateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreateDate));
        private PropertyInfo EntityCreatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreatedBy));
        private PropertyInfo EntityUpdateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdateDate));
        private PropertyInfo EntityUpdatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdatedBy));

        private bool HasGuid => EntityGuidProperty is not null;
        private bool HasDeleted => EntityDeletedProperty is not null;
        private bool HasModified => EntityCreateDateProperty is not null && EntityCreatedByProperty is not null &&
            EntityUpdateDateProperty is not null && EntityUpdatedByProperty is not null;

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

        private readonly IDb _db;

        protected ILogger<Application> Logger { get; }

        protected Application(IDb db, HttpService httpService, ILogger<Application> logger) : base(httpService)
        {
            _db = db;
            Logger = logger;
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
                query = query.Where(entity => EF.Property<bool>(entity, EntityDeletedProperty.Name) == false);
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

        public async Task<Result<List<TResponse>>> List(CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Data().ProjectTo<TEntity, TResponse>(MapperProfile).ToListAsync(cancellationToken);
                if (list.Count > 0)
                    return Success(list, $"{list.Count} {RecordsFound}");
                return Error(list, HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.List(): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<TResponse>> Item(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await Data().ProjectTo<TEntity, TResponse>(MapperProfile).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                if (item is null)
                    return Error(item, HttpStatusCode.NotFound);
                return Success(item, $"1 {RecordsFound}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"ApplicationException: {GetType().Name}.Item(Id = {id}): {exception.Message}");
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
                            {
                                entityEntry.Property(EntityGuidProperty.Name).IsModified = false;
                            }
                            if (HasModified)
                            {
                                entityEntry.Property(EntityCreateDateProperty.Name).IsModified = false;
                                entityEntry.Property(EntityCreatedByProperty.Name).IsModified = false;
                            }
                            break;
                        case EntityState.Deleted:
                            if (HasGuid && HasDeleted)
                            {
                                entityEntry.Property(EntityGuidProperty.Name).IsModified = false;
                            }
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

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
