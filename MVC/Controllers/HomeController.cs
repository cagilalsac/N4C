using Microsoft.AspNetCore.Mvc;
using N4C.App;
using N4C.Controllers;
using System.Net;

namespace MVC.Controllers;

public class HomeController : MvcController
{
    private readonly Application _app;

    public HomeController(Application app)
    {
        _app = app;
    }

    public IActionResult Index()
    {
        SetViewData(_app.Culture);
        return View();
    }

    public IActionResult About()
    {
        SetViewData(_app.Culture);
        return View();
    }

    public IActionResult Error()
    {
        SetViewData(_app.Culture, _app.Error(HttpStatusCode.BadRequest));
        return View("_N4Cmessage");
    }

	public IActionResult Exception()
	{
		SetViewData(_app.Culture, _app.Error(HttpStatusCode.InternalServerError));
		return View("_N4Cexception");
	}
}
