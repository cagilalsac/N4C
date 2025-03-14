using Microsoft.AspNetCore.Mvc;
using N4C.App.Services;
using N4C.Controllers;
using System.Net;

namespace MVC.Controllers;

public class HomeController : MvcController
{
    private readonly HttpService _httpService;

    public HomeController(HttpService httpService)
    {
        _httpService = httpService;
    }

    public IActionResult Index()
    {
        SetViewData(_httpService.Culture);
        return View();
    }

    public IActionResult About()
    {
        SetViewData(_httpService.Culture);
        return View();
    }

    public IActionResult Error()
    {
        SetViewData(_httpService.Culture, _httpService.Error(HttpStatusCode.BadRequest));
        return View("_N4Cmessage");
    }

    public IActionResult Exception()
    {
        SetViewData(_httpService.Culture, _httpService.Error(HttpStatusCode.InternalServerError));
        return View("_N4Cexception");
    }
}
