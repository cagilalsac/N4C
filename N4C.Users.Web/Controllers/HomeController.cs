using Microsoft.AspNetCore.Mvc;
using N4C.Services;

namespace N4C.Users.Web.Controllers;

public class HomeController : Controller
{
    private readonly Service _service;

    public HomeController(Service service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        return View(_service.Result());
    }

    public IActionResult About()
    {
        return View(_service.Result());
    }

    public IActionResult Error()
    {
        return View("_N4Cmessage", _service.Result(false, "Hata", "Error"));
    }

    public IActionResult Exception()
    {
        return View("_N4Cmessage", _service.Result(new Exception("HomeException")));
    }
}
