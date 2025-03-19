using Microsoft.AspNetCore.Mvc;
using N4C.App;
using System.Net;

namespace N4C.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)(result.HttpStatusCode == HttpStatusCode.OK || result.HttpStatusCode == HttpStatusCode.PartialContent ||
                result.HttpStatusCode == HttpStatusCode.Created || result.HttpStatusCode == HttpStatusCode.NoContent ? HttpStatusCode.OK : result.HttpStatusCode), result);
        }

        protected ObjectResult ActionResult<TResponse>(Result<List<TResponse>> result, int? id) where TResponse : class
        {
            if (id.HasValue && result.HttpStatusCode == HttpStatusCode.OK)
                return ActionResult(new Result<TResponse>(HttpStatusCode.NotFound));
            if (result.HttpStatusCode == HttpStatusCode.Created || (id > 0 && (result.HttpStatusCode == HttpStatusCode.PartialContent || result.HttpStatusCode == HttpStatusCode.NoContent)))
                return ActionResult(new Result<TResponse>(result.HttpStatusCode, result.Data?.SingleOrDefault(), result.Message));
            return ActionResult(result);
        }
    }
}
