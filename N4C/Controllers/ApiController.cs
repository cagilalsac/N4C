using Microsoft.AspNetCore.Mvc;
using N4C.App;

namespace N4C.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}
