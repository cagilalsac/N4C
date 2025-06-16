using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LanguagesController : MvcController
    {
        private readonly Service _service;

        public LanguagesController(IModelMetadataProvider modelMetaDataProvider, Service service) : base(modelMetaDataProvider)
        {
            _service = service;
        }

        // GET: Languages
        public IActionResult Index(string culture)
        {
            _service.CreateCookie(Defaults.Culture, culture);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
