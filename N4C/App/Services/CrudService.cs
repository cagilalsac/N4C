using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public abstract class Service<TEntity, TRequest, TResponse> : Service<TEntity, TResponse>
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        private PropertyInfo GuidProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(Entity.Guid));
        private PropertyInfo CreateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreateDate));
        private PropertyInfo CreatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.CreatedBy));
        private PropertyInfo UpdateDateProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdateDate));
        private PropertyInfo UpdatedByProperty => ObjectExtensions.GetPropertyInfo<TEntity>(nameof(IModified.UpdatedBy));
        
        private bool HasGuid => GuidProperty is not null;
        private bool HasModified => CreateDateProperty is not null && CreatedByProperty is not null &&
            UpdateDateProperty is not null && UpdatedByProperty is not null;

        private bool RelationalEntitiesFound { get; set; }
        private List<string> UniqueProperties { get; set; } = ["Name", "UserName", "Title"];
        private ModelStateDictionary ModelState { get; set; } = new ModelStateDictionary();
        private bool Validated { get; set; }
        
        protected string Collation { get; private set; } = "Turkish_CI_AS";
        protected string RecordNotSaved { get; private set; }
        protected string RecordSaved { get; private set; }
        protected string RecordHasRelationalRecords { get; private set; }
        protected string RecordCreated { get; private set; }
        protected string RecordUpdated { get; private set; }
        protected string RecordDeleted { get; private set; }

        protected Service(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }

        protected override void SetCulture(string culture)
        {
            base.SetCulture(culture);
            RecordNotSaved = Culture == Cultures.TR ? "Kaydedilmedi" : "Not saved";
            RecordSaved = Culture == Cultures.TR ? "Kaydedildi. Etkilenen satır sayısı:" : "Saved. Number of rows effected:";
            RecordHasRelationalRecords = Culture == Cultures.TR ? "İlişkili kayıtlar bulunmaktadır" : "Relational records found";
        }

        protected override void SetTitle(string tr, string en = null)
        {
            base.SetTitle(tr, en);
            RecordCreated = Culture == Cultures.TR ? $"{Title} başarıyla oluşturuldu" : $"{Title} created successfully";
            RecordUpdated = Culture == Cultures.TR ? $"{Title} başarıyla güncellendi" : $"{Title} updated successfully";
            RecordDeleted = Culture == Cultures.TR ? $"{Title} başarıyla silindi" : $"{Title} deleted successfully";
        }

        protected void SetCollation(string collation)
        {
            Collation = collation;
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

        protected void AddError(string tr, string en = default)
        {
            ModelState.AddModelError(string.Empty, tr + ";" + (string.IsNullOrWhiteSpace(en) ? string.Empty : en));
        }

        protected TEntity Query(TRequest request)
        {
            if (request.Guid.IsGuid())
                return Query(request.Guid);
            return Query(request.Id);
        }

        public virtual Result<TRequest> Validate(TRequest request, ModelStateDictionary modelState = default, string errorMessagesSeperator = "; ")
        {
            try
            {
                Property entityProperty, requestProperty;
                List<string> errorMessages = new List<string>();
                string errorMessage = string.Empty;
                Result<List<string>> validationResult = Validate(modelState, errorMessagesSeperator);
                if (!validationResult.Success)
                    errorMessages.AddRange(validationResult.Data);
                if (ModelState.Any())
                    errorMessages.AddRange(ModelState.GetErrors(Culture));
                foreach (var uniqueProperty in UniqueProperties)
                {
                    entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                    requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                    if (entityProperty is not null && requestProperty is not null && Query().Any(entity => entity.Id != request.Id &&
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
                Validated = true;
                if (errorMessages.Any())
                {
                    if (!errorMessages.Contains(string.Empty) && (errorMessagesSeperator == "<br>" || errorMessagesSeperator == "<br />" || errorMessagesSeperator == "<br/>"))
                        errorMessages.Insert(0, string.Empty);
                    return Error(request, string.Join(errorMessagesSeperator, errorMessages));
                }
                return Success(request);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.Validate(Request.Id = {request.Id}): {exception.Message}");
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
                if (!Validated)
                {
                    var validationResult = Validate(request);
                    if (!validationResult.Success)
                        return validationResult;
                }
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
                    return Error(request, result);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.Create(Request.Id = {request.Id}): {exception.Message}");
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
                Logger.LogError($"ServiceException: {GetType().Name}.GetItemForEdit(Id = {id}): {exception.Message}");
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
                Logger.LogError($"ServiceException: {GetType().Name}.GetItemForEdit(Guid = {guid}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Validated)
                {
                    var validationResult = Validate(request);
                    if (!validationResult.Success)
                        return validationResult;
                }
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
                Logger.LogError($"ServiceException: {GetType().Name}.Update(Request.Id = {request.Id}): {exception.Message}");
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
                Logger.LogError($"ServiceException: {GetType().Name}.GetItemForDelete(Id = {id}): {exception.Message}");
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
                    return Error(request, result);
                }
                return Success(request, RecordNotSaved);
            }
            catch (Exception exception)
            {
                Logger.LogError($"ServiceException: {GetType().Name}.Delete(Request.Id = {request.Id}): {exception.Message}");
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
                Logger.LogError($"ServiceException: {GetType().Name}.Save(): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        protected Result<TRequest> CreateFiles(TRequest request, TEntity entity)
        {
            if (HasFile && request is FileRequest)
            {
                FileService = new FileService(HttpService, Logger);
                var fileRequest = request as FileRequest;
                var validationResult = FileService.ValidateOtherFiles(fileRequest?.OtherFormFiles);
                if (validationResult.Success)
                {
                    var mainFileResult = FileService.Create(fileRequest.MainFormFile);
                    if (mainFileResult.Success)
                    {
                        var fileEntity = entity as FileEntity;
                        fileEntity.MainFile = mainFileResult.Data.MainFile;
                        var otherFilesResult = FileService.Create(fileRequest.OtherFormFiles);
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
                FileService = new FileService(HttpService, Logger);
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
                FileService = new FileService(HttpService, Logger);
                var fileEntity = entity as FileEntity;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    mainFileResult = FileService.Delete(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult);
                    fileEntity.MainFile = null;
                    otherFilesResult = FileService.Delete(fileEntity.OtherFiles);
                    if (!otherFilesResult.Success)
                        return Error(request, otherFilesResult);
                    fileEntity.OtherFiles = null;
                }
                else if (filePath == fileEntity.MainFile)
                {
                    mainFileResult = FileService.Delete(fileEntity.MainFile);
                    if (!mainFileResult.Success)
                        return Error(request, mainFileResult);
                    fileEntity.MainFile = null;
                }
                else
                {
                    mainFileResult = FileService.Delete(filePath);
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
    }
}
