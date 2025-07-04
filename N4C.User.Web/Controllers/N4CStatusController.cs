#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using N4C.Controllers;
using N4C.Domain;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;
using N4C.User.App.Services;

// Generated from N4C Template.

namespace N4C.User.Web.Controllers
{
    [Authorize(Roles = "system")]
    public class N4CStatusController : MvcController<N4CStatus, N4CStatusRequest, N4CStatusResponse>
    {
        // Service injections:

        /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public N4CStatusController(Service<N4CStatus, N4CStatusRequest, N4CStatusResponse> service

            /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service

            , IModelMetadataProvider modelMetadataProvider
        ) : base(service, modelMetadataProvider)
        {

            /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;

            Set();
        }

        protected override void Set(Action<MvcControllerConfig> config = default)
        {
            base.Set(config => 
            {
                // api parameter can be sent as true to consume the related API or false to use the related service.
                config.SetUri(true, "N4CStatus");

                // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

                /* Can be uncommented and used for many to many relationships. {Entity} must be replaced with the related name in the controller and views. */
                //config.AddViewData("{Entity}Ids", new MultiSelectList(_{Entity}Service.GetResponse<{Entity}Response>(config.GetUri("{Entity}")).Result?.Data ?? _{Entity}Service.GetResponse().Result.Data, "Id", "Name"));
            });
        }

        // GET: N4CStatus
        //[AllowAnonymous]
        public override async Task<IActionResult> Index(PageOrderRequest request)
        {
            // Get collection logic:
            var result = await Service.GetResponse<N4CStatusResponse>(Uri, RefreshTokenUri) ?? await Service.GetResponse(request);

            return View(result);
        }

        // GET: N4CStatus/Details/5
        public override async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await Service.GetResponse<N4CStatusResponse>(Uri, RefreshTokenUri, id) ?? await Service.GetResponse(id);

            return View(result);
        }

        // GET: N4CStatus/Create
        public override async Task<IActionResult> Create()
        {
            // Get item for create logic:
            var result = await Service.GetRequest();
            
            SetViewData();
            return View(result);
        }

        // POST: N4CStatus/Create
        public override async Task<IActionResult> Create(N4CStatusRequest n4CStatusRequest)
        {
            // Insert item logic:
            var result = await Service.Create(Uri, RefreshTokenUri, n4CStatusRequest) ?? await Service.Create(n4CStatusRequest, ModelState);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CStatus/Edit/5
        public override async Task<IActionResult> Edit(int id)
        {
            // Get item for edit logic:
            var result = await Service.GetRequest(id);

            SetViewData();
            return View(result);
        }

        // POST: N4CStatus/Edit
        public override async Task<IActionResult> Edit(N4CStatusRequest n4CStatusRequest)
        {
            // Update item logic:
            var result = await Service.Update(Uri, RefreshTokenUri, n4CStatusRequest) ?? await Service.Update(n4CStatusRequest, ModelState);

            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            SetViewData();
            return View(result);
        }

        // GET: N4CStatus/Delete/5
        public override async Task<IActionResult> Delete(int id)
        {
            // Get item for delete logic:
            var result = await Service.GetResponse(id);

            return View(result);
        }

        // POST: N4CStatus/DeleteConfirmed
        public override async Task<IActionResult> DeleteConfirmed(N4CStatusRequest n4CStatusRequest)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CStatusRequest>(Uri, RefreshTokenUri, n4CStatusRequest.Id) ?? await Service.Delete(n4CStatusRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index));
        }

        // GET: N4CStatus/DeleteByAlertify/5
        public override async Task<IActionResult> DeleteByAlertify(N4CStatusRequest n4CStatusRequest, bool pageOrderSession)
        {
            // Delete item logic:
            var result = await Service.Delete<N4CStatusRequest>(Uri, RefreshTokenUri, n4CStatusRequest.Id) ?? await Service.Delete(n4CStatusRequest);

            SetTempData(result);
            return RedirectToAction(nameof(Index), new { pageOrderSession });
        }
    }
}
