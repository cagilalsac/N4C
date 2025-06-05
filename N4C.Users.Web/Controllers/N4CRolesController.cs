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
    [Authorize(Roles = "system")]
    public class N4CRolesController : MvcController<N4CRole, N4CRoleRequest, N4CRoleResponse>
    {
        // Service injections:

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public N4CRolesController(Service<N4CRole, N4CRoleRequest, N4CRoleResponse> service

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service

            , IModelMetadataProvider modelMetadataProvider
        ) : base(service, modelMetadataProvider)
        {
            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;

            Set(null);
        }

        protected override void Set(Action<ControllerConfig> config)
        {
            base.Set(config => 
            {
                config.SetModelStateErrors(true);

                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

                /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
                //config.AddViewData("{Entity}Ids", new MultiSelectList(_{Entity}Service.GetResponse().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CRoles
        //[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Get collection logic:
            var result = await Service.GetResponse();
            
            return View(result);
        }

        // GET: N4CRoles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CRoles/Create
        public async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CRoles/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(N4CRoleRequest n4CRoleRequest)
        {
            n4CRoleRequest.Set(ModelState);

            // Insert item logic:
            var result = await Service.Create(n4CRoleRequest);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CRoles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CRoles/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(N4CRoleRequest n4CRoleRequest)
        {
            n4CRoleRequest.Set(ModelState);

            // Update item logic:
            var result = await Service.Update(n4CRoleRequest);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CRoles/Delete/5
        [HttpGet, Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CRoles/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(N4CRoleRequest n4CRoleRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CRoleRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CRoles/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(N4CRoleRequest n4CRoleRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CRoleRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }
    }
}
