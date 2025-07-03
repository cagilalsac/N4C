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

        protected Uri Uri { get; private set; }
        protected Uri TokenUri { get; private set; }
        protected Uri RefreshTokenUri { get; private set; }
        protected string RefreshToken { get; private set; }
        protected string Token { get; private set; }

        protected virtual Service Service { get; }

        private readonly IModelMetadataProvider _modelMetaDataProvider;

        protected MvcController(Service service, IModelMetadataProvider modelMetaDataProvider)
        {
            Service = service;
            _modelMetaDataProvider = modelMetaDataProvider;
        }

        protected void Set(string culture, string titleTR, string titleEN)
        {
            Culture = culture.HasNotAny(Settings.Culture);
            Service?.Set(Culture, titleTR, titleEN);
        }

        protected void Set(Uri uri)
        {
            Uri = uri is not null ? new Uri($"{uri.AbsoluteUri}?culture={Culture}") : null;
            TokenUri = null;
            RefreshTokenUri = null;
            RefreshToken = null;
            Token = null;
        }

        protected void Set(Uri uri, Uri refreshTokenUri, Uri tokenUri = default)
        {
            Set(uri);
            RefreshTokenUri = new Uri($"{refreshTokenUri.AbsoluteUri}?culture={Culture}");
            TokenUri = tokenUri is not null ? new Uri($"{tokenUri.AbsoluteUri}?culture={Culture}") : null;
        }

        protected void Set(Uri uri, string token)
        {
            Set(uri);
            Token = token;
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
