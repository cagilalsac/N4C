#nullable disable

using APP.Domain;
using APP.Services.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N4C.App;
using N4C.App.Services;
using N4C.Controllers;

// Generated from N4C Template.

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StoresController : MvcController
    {
        // Service injections:
        private readonly Service<Store, StoreRequest, StoreResponse> _storeService;

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public StoresController(Service<Store, StoreRequest, StoreResponse> storeService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service
        )
        {
            _storeService = storeService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //ViewBag.{Entity}Ids = new MultiSelectList(_{Entity}Service.GetList().Result.Data, "Id", "Name");

            SetViewData(_storeService.Culture, result, _storeService.Title, pageOrder);
        }

        // GET: Stores
        //[AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = pageOrder is null ? await _storeService.GetList() : await _storeService.GetList(pageOrder);
            
            SetViewData(result, pageOrder);
            return View(result);
        }

        // GET: Stores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _storeService.GetItem(id);

            SetViewData(result);
            return View(result);
        }

        // GET: Stores/Create
        public IActionResult Create()
        {
            var result = _storeService.GetItemForCreate();
            SetViewData(result);
            return View(result.Data);
        }

        // POST: Stores/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreRequest storeRequest)
        {
            var result = _storeService.Validate(storeRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Insert item logic:
                result = await _storeService.Create(storeRequest);
                
                if (result.Success)
                {
                    SetTempData(result);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result);
            return View(storeRequest);
        }

        // GET: Stores/Edit/5
        public IActionResult Edit(int id)
        {
            // Get item to edit logic:
            var result = _storeService.GetItemForEdit(id);

            SetViewData(result);
            return View(result.Data);
        }

        // POST: Stores/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StoreRequest storeRequest)
        {
            var result = _storeService.Validate(storeRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Update item logic:
                result = await _storeService.Update(storeRequest);
                
                if (result.Success)
                {
                    SetTempData(result);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result);
            return View(storeRequest);
        }

        // GET: Stores/Delete/5
        public IActionResult Delete(int id)
        {
            // Get item to delete logic:
            var result = _storeService.GetItemForDelete(id);

            SetViewData(result);
            return View(result);
        }

        // POST: Stores/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(StoreRequest storeRequest)
        {
            // Delete item logic:
            var result = await _storeService.Delete(storeRequest);

            SetTempData(result);         
            return RedirectToAction(nameof(Index));
        }

        // GET: Stores/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(StoreRequest storeRequest)
        {
            // Delete item logic:
            var result = await _storeService.Delete(storeRequest);

            SetTempData(result);             
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFile(int id, string path = null)
        {
            // Delete file logic:
            var result = await _storeService.DeleteFiles(id, path);

            SetTempData(result); 
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task ExportToExcel()
        {
            await _storeService.GetExcel();
        }

        public IActionResult Download(string path)
        {
            var result = _storeService.GetFile(path);
            if (result.Success)
                return File(result.Data.FileStream, result.Data.FileContentType, result.Data.FileName);
            SetViewData(result);
            return View("_N4Cmessage");
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class StoreServiceApiController : ServiceApiController<Store, StoreRequest, StoreResponse>
    {
        public StoreServiceApiController(Service<Store, StoreRequest, StoreResponse> storeService) : base(storeService)
        {
        }
    }
}
