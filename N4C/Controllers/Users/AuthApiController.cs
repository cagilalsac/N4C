using Microsoft.AspNetCore.Mvc;
using N4C.App.Services.Auth;
using N4C.App.Services.Auth.Models;

namespace N4C.Controllers.Users
{
    public class AuthApiController : ApiController
    {
        private readonly AuthService _authService;

        public AuthApiController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Token(LoginRequest loginRequest)
        {
            var validationResult = _authService.Validate(ModelState);
            if (validationResult.Success)
            {
                var tokenResult = await _authService.GetJwt(loginRequest);
                return ActionResult(tokenResult);
            }
            return ActionResult(validationResult);
        }
    }
}
