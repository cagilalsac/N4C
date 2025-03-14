using Microsoft.AspNetCore.Mvc;
using N4C.App;

namespace N4C.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}
