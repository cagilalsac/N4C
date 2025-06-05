using Microsoft.AspNetCore.Mvc;
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

        public virtual async Task<IActionResult> DeleteFile(int id, string path = null, string redirectToAction = "Details")
        {
            var result = await Service.DeleteFiles(id, path);
            SetTempData(result);
            return RedirectToAction(redirectToAction, new { id });
        }

        public virtual async Task DownloadExcel()
        {
            await Service.GetExcel();
        }

        public virtual IActionResult DownloadFile(string path)
        {
            var result = Service.GetFile(path);
            if (result.Success)
                return File(result.Data.FileStream, result.Data.FileContentType, result.Data.FileName);
            return View("_N4Cmessage", result);
        }
    }
}
