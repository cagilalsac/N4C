using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Controllers;
using N4C.Services;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;
using N4C.Users.App.Services;

namespace N4C.Users.App.Controllers
{
    public abstract class N4CUsersController<TEntity, TRequest, TResponse> : MvcController<TEntity, TRequest, TResponse>
        where TEntity : N4CUser, new() where TRequest : N4CUserRequest, new() where TResponse : N4CUserResponse, new()
    {
        protected N4CUsersController(Service<TEntity, TRequest, TResponse> service, IModelMetadataProvider modelMetaDataProvider) 
            : base(service, modelMetaDataProvider)
        {
        }

        public virtual async Task<IActionResult> Deactivate(int id)
        {
            var userService = Service as N4CUserService;
            var result = await userService.Deactivate(id);
            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<IActionResult> Activate(string guid)
        {
            var userService = Service as N4CUserService;
            var result = await userService.Activate(guid);
            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous, Route("[action]")]
        public virtual IActionResult Login()
        {
            return View(Service.GetRequest<N4CUserLoginRequest>());
        }

        [HttpPost, AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Login(N4CUserLoginRequest request)
        {
            request.Set(ModelState);
            var result = await (Service as N4CUserService).Login(request);
            if (result.Success)
                return RedirectToAction("Index", "Home");
            return View(result);
        }

        [AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Logout()
        {
            await (Service as N4CUserService).Logout();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous, Route("[action]")]
        public virtual IActionResult Register()
        {
            return View(Service.GetRequest<N4CUserRegisterRequest>());
        }

        [HttpPost, AllowAnonymous, Route("[action]")]
        public virtual async Task<IActionResult> Register(N4CUserRegisterRequest request)
        {
            request.Set(ModelState);
            var result = await (Service as N4CUserService).Register(request);
            if (result.Success)
                return RedirectToAction(nameof(Login));
            return View(result);
        }
    }
}
