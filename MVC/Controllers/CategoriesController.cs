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
    public class CategoriesController : MvcController
    {
        // Service injections:
        private readonly Service<Category, CategoryRequest, CategoryResponse> _categoryService;

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        private readonly HttpService _httpService;

        public CategoriesController(Service<Category, CategoryRequest, CategoryResponse> categoryService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service
        
            , HttpService httpService
        )
        {
            _categoryService = categoryService;
            _httpService = httpService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;

            /* Can be uncommented to use the previously created and related API endpoints instead of related service methods. */
            _httpService.Set(config => config.ApiUri = "categories");
        }

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //ViewBag.{Entity}Ids = new MultiSelectList(_{Entity}Service.Get().Result.Data, "Id", "Name");

            SetViewData(_categoryService.Culture, result, _categoryService.Title, pageOrder);
        }

        // GET: Categories
        //[AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = await _httpService.Get<CategoryResponse>() ?? await _categoryService.Get(pageOrder);
            
            SetViewData(result, pageOrder);
            return View(result);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _categoryService.Get(id);

            SetViewData(result);
            return View(result);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            var result = _categoryService.GetForCreate();
            SetViewData(result);
            return View(result.Data);
        }

        // POST: Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryRequest categoryRequest)
        {
            categoryRequest.Set(ModelState);

            // Insert item logic:
            var result = await _categoryService.Create(categoryRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(categoryRequest);
        }

        // GET: Categories/Edit/5
        public IActionResult Edit(int id)
        {
            // Get item to edit logic:
            var result = _categoryService.GetForEdit(id);

            SetViewData(result);
            return View(result.Data);
        }

        // POST: Categories/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryRequest categoryRequest)
        {
            categoryRequest.Set(ModelState);

            // Update item logic:
            var result = await _categoryService.Update(categoryRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(categoryRequest);
        }

        // GET: Categories/Delete/5
        [HttpGet, Route("[controller]/[action]/{id}")]
        public IActionResult Delete(int id)
        {
            // Get item to delete logic:
            var result = _categoryService.GetForDelete(id);

            SetViewData(result);
            return View(result);
        }

        // POST: Categories/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(CategoryRequest categoryRequest)
        {
            // Delete item logic:
            var result = await _categoryService.Delete(categoryRequest);

            SetTempData(result);         
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(CategoryRequest categoryRequest)
        {
            // Delete item logic:
            var result = await _categoryService.Delete(categoryRequest);

            SetTempData(result);             
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFile(int id, string path = null)
        {
            // Delete file logic:
            var result = await _categoryService.DeleteFiles(id, path);

            SetTempData(result); 
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task ExportToExcel()
        {
            await _categoryService.GetExcel();
        }

        public IActionResult Download(string path)
        {
            var result = _categoryService.GetFile(path);
            if (result.Success)
                return File(result.Data.FileStream, result.Data.FileContentType, result.Data.FileName);
            SetViewData(result);
            return View("_N4Cmessage");
        }
    }
}