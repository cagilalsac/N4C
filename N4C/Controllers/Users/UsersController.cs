using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Users;
using N4C.App.Services.Users.Models;
using N4C.Domain.Users;

namespace N4C.Controllers.Users
{
    [Authorize(Roles = "SystemAdmin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UsersController : MvcController
    {
        // Service injections:
        private readonly Service<User, UserRequest, UserResponse> _userService;
        private readonly Service<Role, RoleRequest, RoleResponse> _roleService;

        public UsersController(Service<User, UserRequest, UserResponse> userService, Service<Role, RoleRequest, RoleResponse> roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            ViewBag.RoleIds = new MultiSelectList(_roleService.Get().Result.Data, "Id", "Name");
            SetViewData(_userService.Culture, result, _userService.Title, pageOrder);
        }

        private async Task<IActionResult> Action(UserViewModel viewModel)
        {
            try
            {
                MvcActions mvcAction = (MvcActions)viewModel.MvcAction;
                Result<UserRequest> requestResult;
                switch (mvcAction)
                {
                    case MvcActions.Details:
                        // Get item logic:
                        var responseResult = await _userService.Get(viewModel.Request.Id);

                        if (responseResult.Success)
                            viewModel.Data = [responseResult.Data];
                        SetViewData(responseResult);
                        break;
                    case MvcActions.CreateGet:
                        // Get item for create logic:
                        requestResult = _userService.GetForCreate();

                        viewModel.Request = requestResult.Data;
                        viewModel.MvcAction = (int)MvcActions.CreatePost;
                        SetViewData(requestResult);
                        break;
                    case MvcActions.EditGet:
                        // Get item for edit logic:
                        requestResult = _userService.GetForEdit(viewModel.Request.Id);

                        viewModel.Request = requestResult.Data;
                        viewModel.MvcAction = (int)MvcActions.EditPost;
                        SetViewData(requestResult);
                        break;
                    case MvcActions.DeleteByAlertify:
                        // Delete item logic:
                        requestResult = await _userService.Delete(viewModel.Request);

                        viewModel.MvcAction = (int)MvcActions.Index;
                        SetTempData(requestResult);
                        break;
                    case MvcActions.CreatePost:
                        viewModel.Request.Set(ModelState);

                        // Insert item logic:
                        requestResult = await _userService.Create(viewModel.Request);

                        if (requestResult.Success)
                        {
                            viewModel.MvcAction = (int)MvcActions.Index;
                            SetTempData(requestResult);
                        }
                        else
                        {
                            SetViewData(requestResult);
                        }
                        break;
                    case MvcActions.EditPost:
                        viewModel.Request.Set(ModelState);

                        // Update item logic:
                        requestResult = await _userService.Update(viewModel.Request);

                        if (requestResult.Success)
                        {
                            viewModel.MvcAction = (int)MvcActions.Index;
                            SetTempData(requestResult);
                        }
                        else
                        {
                            SetViewData(requestResult);
                        }
                        break;
                    default:
                        // Get collection logic:
                        var responseListResult = await _userService.Get();

                        viewModel.Data = responseListResult.Data;
                        SetViewData(responseListResult);
                        break;
                }
                if (mvcAction == MvcActions.CreatePost || mvcAction == MvcActions.EditPost || mvcAction == MvcActions.DeleteByAlertify)
                {
                    if (viewModel.MvcAction == (int)MvcActions.Index)
                        return RedirectToAction(nameof(Index));
                }
                return View("_N4Cusers", viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public async Task<IActionResult> Index()
        {
            return await Action(new UserViewModel() { MvcAction = (int)MvcActions.Index });
        }

        public async Task<IActionResult> Details(UserViewModel viewModel)
        {
            viewModel.MvcAction = (int)MvcActions.Details;
            return await Action(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            return await Action(new UserViewModel() { MvcAction = (int)MvcActions.CreateGet });
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel viewModel)
        {
            viewModel.MvcAction = (int)MvcActions.CreatePost;
            return await Action(viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            return await Action(new UserViewModel() { Request = new UserRequest() { Id = id }, MvcAction = (int)MvcActions.EditGet });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel viewModel)
        {
            viewModel.MvcAction = (int)MvcActions.EditPost;
            return await Action(viewModel);
        }

        public async Task<IActionResult> DeleteByAlertify(int id)
        {
            return await Action(new UserViewModel() { Request = new UserRequest() { Id = id }, MvcAction = (int)MvcActions.DeleteByAlertify });
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            var userService = _userService as UserService;
            var result = await userService.Deactivate(id);
            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Activate(string guid)
        {
            var userService = _userService as UserService;
            var result = await userService.Activate(guid);
            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }
    }
}
