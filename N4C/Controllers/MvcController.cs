using Microsoft.AspNetCore.Mvc;
using N4C.App;
using System.Net;

namespace N4C.Controllers
{
    public abstract class MvcController : Controller
    {
        protected void SetViewData(string culture, string message = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK, string title = default, PageOrder pageOrder = default)
        {
            ViewBag.View = new View(culture, message, httpStatusCode, title, pageOrder);
        }

        protected void SetTempData(string message, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            TempData["Message"] = message;
            TempData["HttpStatusCode"] = (int)httpStatusCode;
        }
    }
}
