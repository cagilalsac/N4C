using Microsoft.AspNetCore.Mvc;
using N4C.App.Services;

namespace N4C.Controllers
{
    public class LanguagesController : Controller
    {
        private readonly HttpService _httpService;

        public LanguagesController(HttpService httpService)
        {
            _httpService = httpService;
        }

        // GET: Languages
        public IActionResult Index(string culture)
        {
            _httpService.SetCookie("Culture", culture);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
