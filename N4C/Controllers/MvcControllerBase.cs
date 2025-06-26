using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class MvcController : Controller
    {
        protected string Culture { get; private set; } = Settings.Culture;

        private Dictionary<string, object> _viewData;

        protected virtual Service Service { get; }

        private readonly IModelMetadataProvider _modelMetaDataProvider;

        protected MvcController(Service service, IModelMetadataProvider modelMetaDataProvider)
        {
            Service = service;
            _modelMetaDataProvider = modelMetaDataProvider;
        }

        protected void Set(string culture, string titleTR = default, string titleEN = default, Dictionary<string, object> viewData = default)
        {
            Culture = culture.HasNotAny(Settings.Culture);
            Service?.Set(false, Culture, titleTR, titleEN);
            _viewData = viewData;
        }

        protected void SetViewData()
        {
            if (_viewData is not null && _viewData.Any())
            {
                ViewData = new ViewDataDictionary(_modelMetaDataProvider, ModelState);
                foreach (var item in _viewData)
                {
                    ViewData[item.Key] = item.Value;
                }
            }
        }

        protected void SetTempData(string tr, string en = default, string key = "Message")
        {
            TempData[key] = Culture == Defaults.TR ? tr : en.HasNotAny(string.Empty);
        }

        protected void SetTempData(Result result, string key = "Message")
        {
            TempData[key] = result.Message;
        }

        public IActionResult Language(string culture)
        {
            Service.CreateCookie(".N4C.Culture", culture);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
