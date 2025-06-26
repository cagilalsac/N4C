using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Controllers;
using N4C.Services;

namespace N4C.Users.Web.Controllers;

public class HomeController : MvcController
{
    public HomeController(Service service, IModelMetadataProvider modelMetaDataProvider) : base(service, modelMetaDataProvider)
    {
    }

    public IActionResult Index()
    {
        return View(Service.Success());
    }

    public IActionResult About()
    {
        return View(Service.Success());
    }

    public IActionResult Error()
    {
        return View("_N4Cmessage", Service.Error());
    }

    public IActionResult Exception()
    {
        return View("_N4Cmessage", Service.Result(new Exception("An exception occured during the operation!")));
    }
}
