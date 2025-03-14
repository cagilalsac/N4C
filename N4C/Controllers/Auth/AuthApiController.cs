using Microsoft.AspNetCore.Mvc;
using N4C.App.Services.Auth;
using N4C.App.Services.Auth.Models;

namespace N4C.Controllers.Auth.API
{
    public class AuthController : ApiController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Token(LoginRequest loginRequest)
        {
            loginRequest.Set(ModelState);

            // Get token logic:
            return ActionResult(await _authService.GetJwt(loginRequest));
        }
    }
}
