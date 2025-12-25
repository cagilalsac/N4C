using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Services;

namespace N4C.Controllers
{
    public class LanguagesController : MvcController
    {
        public LanguagesController(Service service, IModelMetadataProvider modelMetaDataProvider) : base(service, modelMetaDataProvider)
        {
        }

        public IActionResult Index(string culture, string area = "")
        {
            Service.CreateCookie(".N4C.Culture", culture);
            return RedirectToAction("Index", "Home", new { area });
        }
    }
}
