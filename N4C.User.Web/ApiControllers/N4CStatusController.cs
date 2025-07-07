#nullable disable
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.Controllers;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;

// Generated from N4C Template.

namespace N4C.User.Web.ApiControllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "system")]
    public class N4CStatusController : ApiController<N4CStatus, N4CStatusRequest, N4CStatusResponse>
    {
        public N4CStatusController(Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> service) : base(service)
        {
        }

        // GET: N4CStatus
        public override async Task<IActionResult> Get(string pageNumber, string recordsPerPageCount, string orderExpression, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.GetResponse(pageNumber, recordsPerPageCount, orderExpression));
        }

        // GET: N4CStatus/5
        public override async Task<IActionResult> Get(int id, string culture)
        {
            Set(culture);
            return ActionResult(await Service.GetResponse(id));
        }

        // POST: N4CStatus
        public override async Task<IActionResult> Post([FromForm] ApiRequest<N4CStatusRequest> apiRequest, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.Create(apiRequest, ModelState));
        }

        // PUT: N4CStatus
        public override async Task<IActionResult> Put([FromForm] ApiRequest<N4CStatusRequest> apiRequest, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.Update(apiRequest, ModelState));
        }

        // DELETE: N4CStatus/5
        public override async Task<IActionResult> Delete(int id, string culture)
        {
            Set(culture);
            return ActionResult(await Service.Delete(id));
        }
	}
}