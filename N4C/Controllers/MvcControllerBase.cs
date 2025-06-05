using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using N4C.Models;

namespace N4C.Controllers
{
    public abstract class MvcController : Controller
    {
        protected string Culture { get; private set; }

        private Dictionary<string, object> _viewData;

        private readonly IModelMetadataProvider _modelMetaDataProvider;

        protected MvcController(IModelMetadataProvider modelMetaDataProvider)
        {
            _modelMetaDataProvider = modelMetaDataProvider;
            Set(null);
        }

        protected void Set(string culture, Dictionary<string, object> viewData = default)
        {
            Culture = culture ?? Defaults.TR;
            _viewData = viewData ?? new Dictionary<string, object>();
        }

        protected void SetViewData()
        {
            if (_viewData.Any())
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
            TempData[key] = Culture == Defaults.TR ? tr : en ?? string.Empty;
        }

        protected void SetTempData(Result result, string key = "Message")
        {
            TempData[key] = result.Message;
        }
    }
}
