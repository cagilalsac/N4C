using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Domain;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    public abstract class MvcController<TEntity, TRequest, TResponse> : MvcController
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected MvcControllerConfig Config { get; private set; } = new MvcControllerConfig();

        protected override Service<TEntity, TRequest, TResponse> Service { get; }

        protected MvcController(Service<TEntity, TRequest, TResponse> service, IModelMetadataProvider modelMetaDataProvider)
            : base(service, modelMetaDataProvider)
        {
            Service = service;
        }

        protected virtual void Set(Action<MvcControllerConfig> config = default)
        {
            Set(Service.Culture, Service.TitleTR, Service.TitleEN);
            if (config is not null)
            {
                config.Invoke(Config);
                SetViewData(Config.ViewData);
                SetUri(Config.UriDictionaryKey);
            }
        }

        public virtual async Task<IActionResult> Index(PageOrderRequest request)
        {
            var result = await Service.GetResponse(request);
            return View(result);
        }

        public virtual async Task<IActionResult> Details(int id)
        {
            var result = await Service.GetResponse(id);
            return View(result);
        }

        public virtual async Task<IActionResult> Create()
        {
            var result = await Service.GetRequest();
            SetViewData();
            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(TRequest request)
        {
            var result = await Service.Create(request, ModelState);
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            var result = await Service.GetRequest(id);
            SetViewData();
            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(TRequest request)
        {
            var result = await Service.Update(request, ModelState);
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        [Route("[controller]/[action]/{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await Service.GetResponse(id);
            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> DeleteConfirmed(TRequest request)
        {
            var result = await Service.Delete(request);
            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<IActionResult> DeleteByAlertify(TRequest request, bool pageOrderSession)
        {
            var result = await Service.Delete(request);
            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
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
    }
}
