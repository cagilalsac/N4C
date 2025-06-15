#nullable disable
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.Services;
using N4C.Users.App.Controllers;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;

// Generated from N4C Template.

namespace N4C.Users.Web.ApiControllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "system")]
    public class N4CUsersController : N4CUsersApiController
    {
        public N4CUsersController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service) : base(service)
        {
        }

        // GET: N4CUsers
        [HttpGet]
        public override async Task<IActionResult> Get()
        {
            return ActionResult(await Service.GetResponse());
        }

        // GET: N4CUsers/5
        [HttpGet("{id}")]
        public override async Task<IActionResult> Get(int id)
        {
            return ActionResult(await Service.GetResponse(id));
        }

		// POST: N4CUsers
        [HttpPost]
        public override async Task<IActionResult> Post([FromBody] N4CUserRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Create(request));
        }

        // PUT: N4CUsers
        [HttpPut]
        public override async Task<IActionResult> Put([FromBody] N4CUserRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Update(request));
        }

        // DELETE: N4CUsers/5
        [HttpDelete("{id}")]
        public override async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await Service.Delete(id));
        }
	}
}