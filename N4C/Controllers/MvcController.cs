using Microsoft.AspNetCore.Mvc;
using N4C.App;
using System.Net;

namespace N4C.Controllers
{
    public abstract class MvcController : Controller
    {
        protected void SetViewData(string culture, Result result = default, string title = default, PageOrder pageOrder = default)
        {
            ViewBag.View = new View(culture, title, result is null ? string.Empty : result.Message.Replace(";", "<br>"),
                result is null ? HttpStatusCode.OK : result.HttpStatusCode, pageOrder);
        }

        protected void SetTempData(Result result)
        {
            TempData["Message"] = result is null ? string.Empty : result.Message.Replace(";", "<br>");
            TempData["HttpStatusCode"] = (int)(result is null ? HttpStatusCode.OK : result.HttpStatusCode);
        }
    }
}
