using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.Controllers;
using N4C.Services;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;
using N4C.Users.App.Services;

namespace N4C.Users.App.Controllers
{
    public abstract class N4CUsersApiController : ApiController<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        protected N4CUsersApiController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service) : base(service)
        {
        }

        [HttpPost, AllowAnonymous, Route("~/api/[action]")]
        public virtual async Task<IActionResult> Token(N4CTokenRequest request, string culture)
        {
            Set(culture);
            request.Set(ModelState);
            var result = await (Service as N4CUserService).GetToken(request);
            return ActionResult(result);
        }

        [HttpPost, AllowAnonymous, Route("~/api/[action]")]
        public virtual async Task<IActionResult> RefreshToken(N4CRefreshTokenRequest request, string culture)
        {
            Set(culture);
            request.Set(ModelState);
            var result = await (Service as N4CUserService).GetRefreshToken(request);
            return ActionResult(result);
        }
    }
}
