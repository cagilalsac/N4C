#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using N4C.Models;
using N4C.Services;
using N4C.Users.App.Controllers;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;

// Generated from N4C Template.

namespace N4C.Users.Web.Controllers
{
    [Authorize(Roles = "system")]
    public class N4CUsersController : N4CUsersMvcController
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

            Set();
        }

        protected override void Set(Action<MvcControllerConfig> config = default)
        {
            base.Set(config => 
            {
                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):
                config.AddViewData("StatusId", new SelectList(_statusService.GetResponse().Result.Data, "Id", "Title"));

                /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
                config.AddViewData("RoleIds", new MultiSelectList(_N4CRoleService.GetResponse().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CUsers
        //[AllowAnonymous]
        public override async Task<IActionResult> Index(PageOrderRequest request)
        {
            // Get collection logic:
            var result = await Service.GetResponse(request);

            return View(result);
        }

        // GET: N4CUsers/Details/5
        public override async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CUsers/Create
        public override async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CUsers/Create
        public override async Task<IActionResult> Create(N4CUserRequest n4CUserRequest)
        {
            n4CUserRequest.Set(ModelState);

            // Insert item logic:
            var result = await Service.Create(n4CUserRequest);
            
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CUsers/Edit/5
        public override async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CUsers/Edit
        public override async Task<IActionResult> Edit(N4CUserRequest n4CUserRequest)
        {
            n4CUserRequest.Set(ModelState);

            // Update item logic:
            var result = await Service.Update(n4CUserRequest);
            
            if (result.Success)
            {
                 SetTempData(result);
                 return RedirectToAction(nameof(Details), new { id = result.Data.Id });
             }
            SetViewData();
            return View(result);
        }

        // GET: N4CUsers/Delete/5
        public override async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CUsers/DeleteConfirmed
        public override async Task<IActionResult> DeleteConfirmed(N4CUserRequest n4CUserRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CUsers/DeleteByAlertify/5
        public override async Task<IActionResult> DeleteByAlertify(N4CUserRequest n4CUserRequest, bool pageOrderSession)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }
    }
}
