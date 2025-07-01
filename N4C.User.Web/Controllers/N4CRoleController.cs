#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Controllers;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;

// Generated from N4C Template.

namespace N4C.User.Web.Controllers
{
    [Authorize(Roles = "system")]
    public class N4CRoleController : MvcController<N4CRole, N4CRoleRequest, N4CRoleResponse>
    {
        // Service injections:

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public N4CRoleController(Service<N4CRole, N4CRoleRequest, N4CRoleResponse> service

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service

            , IModelMetadataProvider modelMetadataProvider
        ) : base(service, modelMetadataProvider)
        {
            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;

            Set();
        }

        protected override void Set(Action<MvcControllerConfig> config = default)
        {
            base.Set(config => 
            {
                config.SetUri("N4CRole", "https://localhost:7000/api");

                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

                /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
                //config.AddViewData("{Entity}Ids", new MultiSelectList(_{Entity}Service.GetResponse().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CRole
        //[AllowAnonymous]
        public override async Task<IActionResult> Index(PageOrderRequest request)
        {
            // Get collection logic:
            var result = await Service.GetResponse<N4CRoleResponse>(Uri, RefreshTokenUri) ?? await Service.GetResponse(request);

            return View(result);
        }

        // GET: N4CRole/Details/5
        public override async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse<N4CRoleResponse>(Uri, RefreshTokenUri, id) ?? await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CRole/Create
        public override async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CRole/Create
        public override async Task<IActionResult> Create(N4CRoleRequest n4CRoleRequest)
        {
            // Insert item logic:
            var result = await Service.Create(Uri, RefreshTokenUri, n4CRoleRequest) ?? await Service.Create(n4CRoleRequest, ModelState);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CRole/Edit/5
        public override async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CRole/Edit
        public override async Task<IActionResult> Edit(N4CRoleRequest n4CRoleRequest)
        {
            // Update item logic:
            var result = await Service.Update(Uri, RefreshTokenUri, n4CRoleRequest) ?? await Service.Update(n4CRoleRequest, ModelState);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CRole/Delete/5
        public override async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CRole/DeleteConfirmed
        public override async Task<IActionResult> DeleteConfirmed(N4CRoleRequest n4CRoleRequest)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CRoleRequest>(Uri, RefreshTokenUri, n4CRoleRequest.Id) ?? await Service.Delete(n4CRoleRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CRole/DeleteByAlertify/5
        public override async Task<IActionResult> DeleteByAlertify(N4CRoleRequest n4CRoleRequest, bool pageOrderSession)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CRoleRequest>(Uri, RefreshTokenUri, n4CRoleRequest.Id) ?? await Service.Delete(n4CRoleRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }
    }
}
