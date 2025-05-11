#nullable disable

using APP.Domain;
using APP.Services.Models;
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

        private readonly HttpService _httpService;

        public StoresController(Service<Store, StoreRequest, StoreResponse> storeService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service

            , HttpService httpService
        )
        {
            _storeService = storeService;
            _httpService = httpService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;

            /* Can be uncommented to use the previously created and related API endpoints instead of related service methods. */
            _httpService.Set(config => config.ApiUri = "stores");
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //ViewBag.{Entity}Ids = new MultiSelectList(_{Entity}Service.Get().Result.Data, "Id", "Name");

            SetViewData(_storeService.Culture, result, _storeService.Title, pageOrder);
        }

        // GET: Stores
        //[AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = await _httpService.Get<StoreResponse>() ?? await _storeService.Get(pageOrder);
            
            SetViewData(result, pageOrder);
            return View(result);
        }

        // GET: Stores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _storeService.Get(id);

            SetViewData(result);
            return View(result);
        }

        // GET: Stores/Create
        public IActionResult Create()
        {
            var result = _storeService.GetForCreate();
            SetViewData(result);
            return View(result.Data);
        }

        // POST: Stores/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreRequest storeRequest)
        {
            storeRequest.Set(ModelState);

            // Insert item logic:
            var result = await _storeService.Create(storeRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(storeRequest);
        }

        // GET: Stores/Edit/5
        public IActionResult Edit(int id)
        {
            // Get item to edit logic:
            var result = _storeService.GetForEdit(id);

            SetViewData(result);
            return View(result.Data);
        }

        // POST: Stores/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StoreRequest storeRequest)
        {
            storeRequest.Set(ModelState);

            // Update item logic:
            var result = await _storeService.Update(storeRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(storeRequest);
        }

        // GET: Stores/Delete/5
        [HttpGet, Route("[controller]/[action]/{id}")]
        public IActionResult Delete(int id)
        {
            // Get item to delete logic:
            var result = _storeService.GetForDelete(id);

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
}
