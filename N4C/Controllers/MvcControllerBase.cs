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
        protected string Culture { get; private set; }
        protected string Title { get; private set; }

        private Dictionary<string, object> _viewData;

        protected Uri Uri { get; private set; }
        protected Uri TokenUri { get; private set; }
        protected Uri RefreshTokenUri { get; private set; }
        protected string Token { get; private set; }

        protected virtual Service Service { get; }

        private readonly IModelMetadataProvider _modelMetaDataProvider;

        protected MvcController(Service service, IModelMetadataProvider modelMetaDataProvider)
        {
            Service = service;
            _modelMetaDataProvider = modelMetaDataProvider;
            Set();
        }

        protected void Set(string culture = default, string titleTR = default, string titleEN = default)
        {
            Service?.Set(culture, titleTR, titleEN);
            Culture = Service?.Culture.HasNotAny(Settings.Culture);
            Title = Culture == Defaults.TR ? Service?.TitleTR : Service?.TitleEN;
        }

        protected void SetUri(string uriDictionaryKey)
        {
            if (uriDictionaryKey.HasAny())
                Uri = new Uri($"{uriDictionaryKey.GetUri()}?culture={Culture}");
            TokenUri = "token".GetUri();
            if (TokenUri is not null)
                TokenUri = new Uri($"{TokenUri}?culture={Culture}");
            RefreshTokenUri = "refreshtoken".GetUri();
            if (RefreshTokenUri is not null)
                RefreshTokenUri = new Uri($"{RefreshTokenUri}?culture={Culture}");
            Token = null;
        }

        protected void SetUri(string uri, string token = default)
        {
            Uri = new Uri($"{uri}?culture={Culture}");
            Token = token;
            TokenUri = null;
            RefreshTokenUri = null;
        }

        protected void SetViewData(Dictionary<string, object> viewData)
        {
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
    }
}
