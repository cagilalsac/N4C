#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Controllers;
using N4C.User.App.Domain;
using N4C.User.App.Models;

// Generated from N4C Template.

namespace N4C.User.Web.Controllers
{
    [Authorize(Roles = "admin,system")]
    public class N4CUserController : N4CUserMvcController
    {
        // Service injections:
        private readonly Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> _statusService;

        /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
        private readonly Service<N4CRole, N4CRoleRequest, N4CRoleResponse> _N4CRoleService;

        public N4CUserController(Service<N4CUser, N4CUserRequest, N4CUserResponse> service
            , Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> statusService

            /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
            , Service<N4CRole, N4CRoleRequest, N4CRoleResponse> N4CRoleService

            , IModelMetadataProvider modelMetadataProvider
        ) : base(service, modelMetadataProvider)
        {
            _statusService = statusService;

            /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
            _N4CRoleService = N4CRoleService;

            Set();
        }

        protected override void Set(Action<MvcControllerConfig> config = default)
        {
            base.Set(config => 
            {
                // api parameter can be sent as true to consume the related API or false to use the related service.
                config.SetUri(true, "N4CUser");

                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):
                config.AddViewData("StatusId", new SelectList(_statusService.GetResponse<N4CStatusResponse>(config.GetUri("N4CStatus")).Result?.Data ?? 
                    _statusService.GetResponse().Result.Data, "Id", "Title"));

                /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
                config.AddViewData("RoleIds", new MultiSelectList(_N4CRoleService.GetResponse<N4CRoleResponse>(config.GetUri("N4CRole")).Result?.Data ?? 
                    _N4CRoleService.GetResponse().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CUser
        //[AllowAnonymous]
        public override async Task<IActionResult> Index(PageOrderRequest request)
        {
            // Get collection logic:
            var result = await Service.GetResponse<N4CUserResponse>(Uri, RefreshTokenUri, request) ?? await Service.GetResponse(request);

            return View(result);
        }

        // GET: N4CUser/Details/5
        public override async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse<N4CUserResponse>(Uri, RefreshTokenUri, id) ?? await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CUser/Create
        public override async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CUser/Create
        public override async Task<IActionResult> Create(N4CUserRequest n4CUserRequest)
        {
            // Insert item logic:
            var result = await Service.Create(Uri, RefreshTokenUri, n4CUserRequest) ?? await Service.Create(n4CUserRequest, ModelState);
            
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CUser/Edit/5
        public override async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CUser/Edit
        public override async Task<IActionResult> Edit(N4CUserRequest n4CUserRequest)
        {
            // Update item logic:
            var result = await Service.Update(Uri, RefreshTokenUri, n4CUserRequest) ?? await Service.Update(n4CUserRequest, ModelState);
            
            if (result.Success)
            {
                 SetTempData(result);
                 return RedirectToAction(nameof(Details), new { id = result.Data.Id });
             }
            SetViewData();
            return View(result);
        }

        // GET: N4CUser/Delete/5
        public override async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CUser/DeleteConfirmed
        public override async Task<IActionResult> DeleteConfirmed(N4CUserRequest n4CUserRequest)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CUserRequest>(Uri, RefreshTokenUri, n4CUserRequest.Id) ?? await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CUser/DeleteByAlertify/5
        public override async Task<IActionResult> DeleteByAlertify(N4CUserRequest n4CUserRequest, bool pageOrderSession)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CUserRequest>(Uri, RefreshTokenUri, n4CUserRequest.Id) ?? await Service.Delete(n4CUserRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }
    }
}
