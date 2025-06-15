using Microsoft.AspNetCore.Mvc;
using N4C.Models;
using System.Net;

namespace N4C.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)(result.Success ? HttpStatusCode.OK : result.HttpStatusCode), result);
        }
    }
}
