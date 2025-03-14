using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Users.Models;
using N4C.Domain.Users;

namespace N4C.Controllers.Users
{
    [Authorize(Roles = "SystemAdmin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RolesController : MvcController
    {
        // Service injections:
        private readonly Service<Role, RoleRequest, RoleResponse> _roleService;

        public RolesController(Service<Role, RoleRequest, RoleResponse> roleService)
        {
            _roleService = roleService;
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            SetViewData(_roleService.Culture, result, _roleService.Title, pageOrder);
        }

        public async Task<IActionResult> Index(RoleViewModel viewModel)
        {
            try
            {
                MvcActions mvcAction = (MvcActions)viewModel.MvcAction;
                Result<RoleRequest> requestResult;
                switch (mvcAction)
                {
                    case MvcActions.Details:
                        // Get item logic:
                        var responseResult = await _roleService.GetItem(viewModel.Request.Id);

                        if (responseResult.Success)
                            viewModel.Data = [responseResult.Data];
                        SetViewData(responseResult);
                        break;
                    case MvcActions.CreateGet:
                        // Get item for create logic:
                        requestResult = _roleService.GetItemForCreate();

                        viewModel.Request = requestResult.Data;
                        viewModel.MvcAction = (int)MvcActions.CreatePost;
                        SetViewData(requestResult);
                        break;
                    case MvcActions.EditGet:
                        // Get item for edit logic:
                        requestResult = _roleService.GetItemForEdit(viewModel.Request.Id);

                        viewModel.Request = requestResult.Data;
                        viewModel.MvcAction = (int)MvcActions.EditPost;
                        SetViewData(requestResult);
                        break;
                    case MvcActions.DeleteGet:
                        // Delete item logic:
                        requestResult = await _roleService.Delete(viewModel.Request);

                        viewModel.MvcAction = (int)MvcActions.Index;
                        SetTempData(requestResult);
                        break;
                    case MvcActions.CreatePost:
                        viewModel.Request.Set(ModelState);

                        // Insert item logic:
                        requestResult = await _roleService.Create(viewModel.Request);

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
                        requestResult = await _roleService.Update(viewModel.Request);

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
                        var responseListResult = await _roleService.GetList();

                        viewModel.Data = responseListResult.Data;
                        SetViewData(responseListResult);
                        break;
                }
                if (mvcAction == MvcActions.CreatePost || mvcAction == MvcActions.EditPost || mvcAction == MvcActions.DeleteGet)
                {
                    if (viewModel.MvcAction == (int)MvcActions.Index)
                        return RedirectToAction(nameof(Index));
                }
                return View("_N4Croles", viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }
    }
}
