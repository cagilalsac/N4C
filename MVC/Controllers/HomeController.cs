using Microsoft.AspNetCore.Mvc;
using N4C.App.Services;
using N4C.Controllers;
using System.Net;

namespace MVC.Controllers;

public class HomeController : MvcController
{
    private readonly Service _service;

    public HomeController(Service service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        SetViewData(_service.Culture);
        return View();
    }

    public IActionResult About()
    {
        SetViewData(_service.Culture);
        return View();
    }

    public IActionResult Error()
    {
        SetViewData(_service.Culture, _service.Error(HttpStatusCode.BadRequest));
        return View("_N4Cmessage");
    }

    public IActionResult Exception()
    {
        SetViewData(_service.Culture, _service.Error(HttpStatusCode.InternalServerError));
        return View("_N4Cexception");
    }
}
