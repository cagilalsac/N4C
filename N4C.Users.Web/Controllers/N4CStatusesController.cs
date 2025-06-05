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
    public class N4CStatusesController : MvcController<N4CStatus, N4CStatusRequest, N4CStatusResponse>
    {
        // Service injections:

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public N4CStatusesController(Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> service

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
                //config.AddViewData("{Entity}Ids", new MultiSelectList(_{Entity}Service.Responses().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CStatuses
        //[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Get collection logic:
            var result = await Service.GetResponse();
            
            return View(result);
        }

        // GET: N4CStatuses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CStatuses/Create
        public async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CStatuses/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(N4CStatusRequest n4CStatusRequest)
        {
            n4CStatusRequest.Set(ModelState);

            // Insert item logic:
            var result = await Service.Create(n4CStatusRequest);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CStatuses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CStatuses/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(N4CStatusRequest n4CStatusRequest)
        {
            n4CStatusRequest.Set(ModelState);

            // Update item logic:
            var result = await Service.Update(n4CStatusRequest);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CStatuses/Delete/5
        [HttpGet, Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CStatuses/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(N4CStatusRequest n4CStatusRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CStatusRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CStatuses/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(N4CStatusRequest n4CStatusRequest)
        {
            // Delete item logic:
            var result = await Service.Delete(n4CStatusRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }
    }
}
