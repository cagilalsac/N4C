using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.Controllers;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;
using N4C.User.App.Services;

namespace N4C.User.App.Controllers
{
    public abstract class N4CUserApiController : ApiController<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        protected N4CUserApiController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service) : base(service)
        {
        }

        [HttpPost, AllowAnonymous, Route("~/api/[action]")]
        public virtual async Task<IActionResult> Token(TokenRequest request, string culture)
        {
            Set(culture);
            var result = await (Service as N4CUserService).GetToken(request, ModelState);
            return ActionResult(result);
        }

        [HttpPost, AllowAnonymous, Route("~/api/[action]")]
        public virtual async Task<IActionResult> RefreshToken(RefreshTokenRequest request, string culture)
        {
            Set(culture);
            var result = await (Service as N4CUserService).GetRefreshToken(request, ModelState);
            return ActionResult(result);
        }
    }
}
