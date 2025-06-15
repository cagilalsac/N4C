#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using N4C.Services;
using N4C.Controllers;
using N4C.Users.App.Models;
using N4C.Users.App.Domain;
using N4C.Models;

// Generated from N4C Template.

namespace N4C.Users.Web.ApiControllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class N4CRolesController : ApiController<N4CRole, N4CRoleRequest, N4CRoleResponse>
    {
        public N4CRolesController(Service<N4CRole, N4CRoleRequest, N4CRoleResponse> service) : base(service)
        {
        }

        // GET: N4CRoles
        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            return ActionResult(await Service.GetResponse());
        }

        // GET: N4CRoles/5
        [HttpGet("{id}")]
        public override async Task<IActionResult> Get(int id)
        {
            return ActionResult(await Service.GetResponse(id));
        }

		// POST: N4CRoles
        [HttpPost]
        public override async Task<IActionResult> Post([FromBody] N4CRoleRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Create(request));
        }

        // PUT: N4CRoles
        [HttpPut]
        public override async Task<IActionResult> Put([FromBody] N4CRoleRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Update(request));
        }

        // DELETE: N4CRoles/5
        [HttpDelete("{id}")]
        public override async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await Service.Delete(id));
        }
	}
}