using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Domain;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    public abstract class MvcController<TEntity, TRequest, TResponse> : MvcController where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected ControllerConfig Config { get; private set; } = new ControllerConfig();

        protected Service<TEntity, TRequest, TResponse> Service { get; }

        protected MvcController(Service<TEntity, TRequest, TResponse> service, IModelMetadataProvider modelMetaDataProvider) : base(modelMetaDataProvider)
        {
            Service = service;
        }

        protected virtual void Set(Action<ControllerConfig> config)
        {
            config.Invoke(Config);
            Service.SetModelStateErrors(Config.ModelStateErrors);
            Config.SetCulture(Service.Culture);
            Set(Config.Culture, Config.ViewData);
        }
    }
}
