using Microsoft.AspNetCore.Mvc;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;
using System.Net;

namespace N4C.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected string Culture { get; private set; } = Settings.Culture;

        protected virtual Service Service { get; }

        protected ApiController(Service service)
        {
            Service = service;
        }

        protected virtual void Set(string culture = default)
        {
            if (culture.HasAny())
                Culture = culture.Split('-').First().ToLower() == Defaults.TR.Split('-').First() ? Defaults.TR : Defaults.EN;
            Service?.Set(true, Culture.HasNotAny(Settings.Culture));
        }

        protected ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)(result.Success ? HttpStatusCode.OK : result.HttpStatusCode), result);
        }
    }
}
