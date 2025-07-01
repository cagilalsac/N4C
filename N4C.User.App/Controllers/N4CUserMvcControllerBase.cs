using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Controllers;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;
using N4C.User.App.Services;

namespace N4C.User.App.Controllers
{
    public abstract class N4CUserMvcController : MvcController<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        protected N4CUserMvcController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service, IModelMetadataProvider modelMetaDataProvider)
            : base(service, modelMetaDataProvider)
        {
        }

        public virtual async Task<IActionResult> Deactivate(int id, bool pageOrderSession)
        {
            var result = await (Service as N4CUserService).Deactivate(id);
            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }

        public virtual async Task<IActionResult> Activate(string guid, bool pageOrderSession)
        {
            var result = await (Service as N4CUserService).Activate(guid);
            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }

        [AllowAnonymous, Route("[action]")]
        public virtual IActionResult Login()
        {
            return View(Service.GetRequest<N4CUserLoginRequest>());
        }

        [HttpPost, AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Login(N4CUserLoginRequest request)
        {
            var result = await (Service as N4CUserService).Login(request, ModelState, TokenUri);
            if (result.Success)
                return RedirectToAction("Index", "Home", new { area = "" });
            return View(result);
        }

        [AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Logout()
        {
            await Service.Logout();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [AllowAnonymous, Route("[action]")]
        public virtual IActionResult Register()
        {
            return View(Service.GetRequest<N4CUserRegisterRequest>());
        }

        [HttpPost, AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Register(N4CUserRegisterRequest request)
        {
            var result = await (Service as N4CUserService).Register(request, ModelState);
            if (result.Success)
                return RedirectToAction(nameof(Login));
            return View(result);
        }
    }
}
