using Microsoft.AspNetCore.Mvc;
using N4C.App;
using N4C.App.Services.Auth;
using N4C.App.Services.Auth.Models;

namespace N4C.Controllers.Users
{
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : MvcController
    {
        // Service injections:
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            SetViewData(_authService.Culture, result, _authService.Title, pageOrder);
        }

        [Route("{AuthMvcAction}")]
        public async Task<IActionResult> Index(AuthViewModel viewModel)
        {
            try
            {
                AuthMvcActions authMvcAction = (AuthMvcActions)viewModel.AuthMvcAction;
                Result result;
                bool redirectToHome = false;
                bool redirectToLogin = false;
                switch (authMvcAction)
                {
                    case AuthMvcActions.LoginGet:
                        viewModel.AuthMvcAction = (int)AuthMvcActions.LoginPost;
                        SetViewData(_authService.Culture);
                        break;
                    case AuthMvcActions.RegisterGet:
                        viewModel.AuthMvcAction = (int)AuthMvcActions.RegisterPost;
                        SetViewData(_authService.Culture);
                        break;
                    case AuthMvcActions.LoginPost:
                        result = _authService.Validate(ModelState, "<br>");
                        if (result.Success)
                        {
                            // Login logic:
                            result = await _authService.Login(viewModel.LoginRequest);

                            if (result.Success)
                            {
                                redirectToHome = true;
                                SetTempData(result);
                            }
                        }
                        if (!redirectToHome)
                            SetViewData(result);
                        break;
                    case AuthMvcActions.RegisterPost:
                        result = _authService.Validate(ModelState, "<br>");
                        if (result.Success)
                        {
                            // Register logic:
                            result = await _authService.Register(viewModel.RegisterRequest);

                            if (result.Success)
                            {
                                redirectToLogin = true;
                                SetTempData(result);
                            }
                        }
                        if (!redirectToLogin)
                            SetViewData(result);
                        break;
                    default:
                        redirectToLogin = true;
                        await _authService.Logout();
                        break;
                }
                if (redirectToHome)
                    return RedirectToAction("Index", "Home", new { area = "" });
                else if (redirectToLogin)
                    return RedirectToAction("Index", new { AuthMvcAction = (int)AuthMvcActions.LoginGet });
                return View("_N4Cauth", viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }
    }
}
