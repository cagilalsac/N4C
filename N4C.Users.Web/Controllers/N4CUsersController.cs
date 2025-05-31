#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using N4C.Models;
using N4C.Services;
using N4C.Controllers;
using N4C.Users.App.Models;
using N4C.Users.App.Domain;

// Generated from N4C Template.

namespace N4C.Users.Web.Controllers
{
    //[Authorize]
    public class N4CUsersController : MvcController<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        // Service injections:
        private readonly Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> _statusService;

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        private readonly Service<N4CRole, N4CRoleRequest, N4CRoleResponse> _N4CRoleService;

        public N4CUsersController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service
            , Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> statusService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            , Service<N4CRole, N4CRoleRequest, N4CRoleResponse> N4CRoleService

            , IModelMetadataProvider modelMetadataProvider
        ) : base(service, modelMetadataProvider)
        {
            _statusService = statusService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            _N4CRoleService = N4CRoleService;

            Set(null);
        }

        protected override void Set(Action<ControllerConfig> config)
        {
            base.Set(config => 
            {
                config.SetModelStateErrors(false);

                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):
                config.AddViewData("StatusId", new SelectList(_statusService.Responses().Result.Data, "Id", "Title"));

                /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
                config.AddViewData("RoleIds", new MultiSelectList(_N4CRoleService.Responses().Result.Data.Where(response => response.Id != (int)N4CRoles.System), "Id", "Name"));
            });
        }

        // GET: N4CUsers
        //[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Get collection logic:
            var result = await Service.Responses();
            
            return View(result);
        }

        // GET: N4CUsers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.Response(id);

            return View(result);
        }

        // GET: N4CUsers/Create
        public async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.Request();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CUsers/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(N4CUserRequest n4CUserRequest)
        {
            n4CUserRequest.Set(ModelState);

            // Insert item logic:
            var result = await Service.Create(n4CUserRequest);
            
            if (result.Success)
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            SetViewData();
            return View(result);
        }

        // GET: N4CUsers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.Request(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CUsers/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(N4CUserRequest n4CUserRequest)
        {
            n4CUserRequest.Set(ModelState);

            // Update item logic:
            var result = await Service.Update(n4CUserRequest);
            
            if (result.Success)
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            SetViewData();
            return View(result);
        }

        // GET: N4CUsers/Delete/5
        [HttpGet, Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.Response(id);

            return View(result);
        }

        // POST: N4CUsers/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(N4CUserRequest n4CUserRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CUsers/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(N4CUserRequest n4CUserRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }
    }
}
