using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;

namespace N4C.Services
{
    public abstract class Service<TEntity, TRequest, TResponse> : Service, IDisposable
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected ServiceConfig<TEntity, TRequest, TResponse> Config { get; private set; } = new ServiceConfig<TEntity, TRequest, TResponse>();

        private bool RelationsFound { get; set; }

        private DbContext Db { get; }

        protected Service(DbContext db, IHttpContextAccessor httpContextAccessor, ILogger<Service> logger) : base(httpContextAccessor, logger)
        {
            Db = db;
            Set(null);
        }

        protected virtual void Set(Action<ServiceConfig<TEntity, TRequest, TResponse>> config)
        {
            config.Invoke(Config);
            Set(Config.Culture, Config.TitleTR, (Config.TitleEN ?? "Record") == "Record" ? typeof(TEntity).Name : Config.TitleEN);
        }

        protected virtual IQueryable<TEntity> Entities()
        {
            var query = Config.NoTracking ? Db.Set<TEntity>().AsNoTracking() : Db.Set<TEntity>();
            if (Config.SqlServer && Config.SplitQuery)
                query = query.AsSplitQuery();
            return query.Where(entity => (EF.Property<bool?>(entity, nameof(Domain.Entity.Deleted)) ?? false) == false);
        }

        protected TEntity Entity(TRequest request, CancellationToken cancellationToken = default)
        {
            TEntity item = null;
            try
            {
                item = Entities().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)?.Result ?? new TEntity();
            }
            catch (Exception exception)
            {
                LogError($"ServiceException: {exception.Message}");
            }
            return item;
        }

        public virtual async Task<Result<List<TResponse>>> Responses(CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            try
            {
                list = await Entities().Map<TEntity, TResponse>(Config).ToListAsync(cancellationToken);
                return Found(list);
            }
            catch (Exception exception)
            {
                return Error(list, exception);
            }
        }

        public virtual async Task<Result<TResponse>> Response(int id, CancellationToken cancellationToken = default)
        {
            TResponse item = null;
            try
            {
                item = await Entities().Map<TEntity, TResponse>(Config).SingleOrDefaultAsync(response => response.Id == id, cancellationToken);
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(item, exception);
            }
        }

        public virtual async Task<Result<TRequest>> Request(int? id = default, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                if (id.HasValue)
                    item = await Entities().Map<TEntity, TRequest>(Config).SingleOrDefaultAsync(request => request.Id == id.Value, cancellationToken);
                else
                    item = new TRequest();
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(item, exception);
            }
        }

        public virtual async Task<Result<TRequest>> Request(string guid, CancellationToken cancellationToken = default)
        {
            TRequest item = null;
            try
            {
                item = await Entities().Map<TEntity, TRequest>(Config).SingleOrDefaultAsync(request => request.Guid == guid, cancellationToken);
                return Found(item);
            }
            catch (Exception exception)
            {
                return Error(item, exception);
            }
        }

        protected virtual Result<TRequest> Validate(TRequest request)
        {
            try
            {
                Property entityProperty, requestProperty;
                string collation = "Turkish_CI_AS";
                string modelStateErrors = Validate(request.ModelState).Message;
                string uniquePropertyError = string.Empty;
                if (Config.SqlServer)
                {
                    foreach (var uniqueProperty in Config.UniqueProperties)
                    {
                        entityProperty = ObjectExtensions.GetProperty<TEntity>(uniqueProperty);
                        requestProperty = ObjectExtensions.GetProperty(uniqueProperty, true, request);
                        if (entityProperty is not null && requestProperty is not null && Entities().Any(entity => entity.Id != request.Id &&
                            EF.Functions.Collate(EF.Property<string>(entity, entityProperty.Name), collation) == EF.Functions.Collate((requestProperty.Value ?? "").ToString(), collation).Trim()))
                        {
                            if (Culture == Cultures.TR)
                                uniquePropertyError = $"{requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} değerine sahip {Config.Title.ToLower()} bulunmaktadır!";
                            else
                                uniquePropertyError = $"{Config.Title} with the same value for {requestProperty.DisplayName.GetDisplayName(requestProperty.Name, Culture)} exists!";
                            break;
                        }
                    }
                }
                return Validated(request, modelStateErrors, uniquePropertyError);
            }
            catch (Exception exception)
            {
                return Error(request, exception);
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
                if (!Config.SoftDelete)
                    Db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
            }
        }

        public virtual async Task<Result<TRequest>> Create(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return validationResult;
                var entity = request.Map<TRequest, TEntity>(Config).Trim();
                entity.Guid = Guid.NewGuid().ToString();
                entity.CreateDate = DateTime.Now;
                entity.CreatedBy = GetUserName();
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
                return Error(request, exception);
            }
        }

        public virtual async Task<Result<TRequest>> Update(TRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = Validate(request);
                if (!validationResult.Success)
                    return validationResult;
                var entity = Db.Set<TEntity>().Find(request.Id);
                if (entity is null)
                    return NotFound(request);
                entity = request.Map(Config, entity).Trim();
                entity.UpdateDate = DateTime.Now;
                entity.UpdatedBy = GetUserName();
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
                return Error(request, exception);
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
                if (Config.SoftDelete)
                {
                    entity.Deleted = true;
                    entity.UpdateDate = DateTime.Now;
                    entity.UpdatedBy = GetUserName();
                    Db.Set<TEntity>().Update(entity);
                }
                else
                {
                    Db.Set<TEntity>().Remove(entity);
                }
                if (save)
                    await Save(cancellationToken);
                return Deleted(request);
            }
            catch (Exception exception)
            {
                return Error(request, exception);
            }
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
