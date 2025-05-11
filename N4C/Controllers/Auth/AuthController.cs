using Microsoft.AspNetCore.Mvc;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Auth;
using N4C.App.Services.Auth.Models;

namespace N4C.Controllers.Auth
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : MvcController
    {
        // Service injections:
        private readonly AuthService _authService;
        private readonly HttpService _httpService;

        public AuthController(AuthService authService, HttpService httpService)
        {
            _authService = authService;
            _httpService = httpService;
            _httpService.Set(c => c.ApiUri = "token");
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            SetViewData(_authService.Culture, result);
        }

        private async Task<IActionResult> Action(AuthViewModel viewModel)
        {
            try
            {
                AuthMvcActions authMvcAction = (AuthMvcActions)viewModel.AuthMvcAction;
                Result result;
                bool redirectToHome = false;
                bool redirectToLogin = false;
                switch (authMvcAction)
                {
                    case AuthMvcActions.LogoutGet:
                        redirectToLogin = true;
                        await _authService.Logout();
                        if (!string.IsNullOrWhiteSpace(_httpService.ApiUri))
                            _httpService.DeleteCookie("Token");
                        break;
                    case AuthMvcActions.RegisterGet:
                        viewModel.AuthMvcAction = (int)AuthMvcActions.RegisterPost;
                        SetViewData(_authService.Culture);
                        break;
                    case AuthMvcActions.LoginPost:
                        viewModel.LoginRequest.Set(ModelState);

                        // Login logic:
                        result = await _authService.Login(viewModel.LoginRequest);

                        if (result.Success)
                        {
                            redirectToHome = true;
                            SetTempData(result);
                            if (!string.IsNullOrWhiteSpace(_httpService.ApiUri))
                            {
                                var tokenResult = await _authService.GetToken(viewModel.LoginRequest);
                                if (tokenResult.Success)
                                    _httpService.CreateCookie("Token", tokenResult.Data.Token);
                            }
                        }
                        else
                        {
                            SetViewData(result);
                        }
                        break;
                    case AuthMvcActions.RegisterPost:
                        viewModel.RegisterRequest.Set(ModelState);

                        // Register logic:
                        result = await _authService.Register(viewModel.RegisterRequest);

                        if (result.Success)
                        {
                            redirectToLogin = true;
                            SetTempData(result);
                        }
                        else
                        {
                            SetViewData(result);
                        }
                        break;
                    default:
                        viewModel.AuthMvcAction = (int)AuthMvcActions.LoginPost;
                        SetViewData(_authService.Culture);
                        break;
                }
                if (redirectToHome)
                    return RedirectToAction("Index", "Home", new { area = "" });
                else if (redirectToLogin)
                    return RedirectToAction(nameof(Login));
                return View("_N4Cauth", viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [Route("[action]")]
        public async Task<IActionResult> Login()
        {
            return await Action(new AuthViewModel() { AuthMvcAction = (int)AuthMvcActions.LoginGet });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login(AuthViewModel viewModel)
        {
            viewModel.AuthMvcAction = (int)AuthMvcActions.LoginPost;
            return await Action(viewModel);
        }

        [Route("[action]")]
        public async Task<IActionResult> Register()
        {
            return await Action(new AuthViewModel() { AuthMvcAction = (int)AuthMvcActions.RegisterGet });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Register(AuthViewModel viewModel)
        {
            viewModel.AuthMvcAction = (int)AuthMvcActions.RegisterPost;
            return await Action(viewModel);
        }

        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            return await Action(new AuthViewModel() { AuthMvcAction = (int)AuthMvcActions.LogoutGet });
        }
    }
}
