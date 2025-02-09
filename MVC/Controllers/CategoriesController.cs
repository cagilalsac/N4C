#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using N4C.App.Services;
using N4C.App;
using N4C.Controllers;
using APP.Services.Models;
using APP.Domain;

// Generated from N4C Template.

namespace MVC.Controllers
{
    //[Authorize]
    public class CategoriesController : MvcController
    {
        // Service injections:
        private readonly Service<Category, CategoryRequest, CategoryResponse> _categoryService;

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        //private readonly Service<{Entity}, {Entity}Request, {Entity}Response> _{Entity}Service;

        public CategoriesController(Service<Category, CategoryRequest, CategoryResponse> categoryService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //, Service<{Entity}, {Entity}Request, {Entity}Response> {Entity}Service
        )
        {
            _categoryService = categoryService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //_{Entity}Service = {Entity}Service;
        }

        void SetViewData(string message = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            //ViewBag.{Entity}Ids = new MultiSelectList(_{Entity}Service.GetList().Result.Data, "Id", "Name");

            SetViewData(_categoryService.Culture, message, httpStatusCode, _categoryService.Title, pageOrder);
        }

        // GET: Categories
        //[AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = pageOrder is null ? await _categoryService.GetList() : await _categoryService.GetList(pageOrder);
            
            SetViewData(result.Message, result.HttpStatusCode, pageOrder);
            return View(result);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _categoryService.GetItem(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            var result = _categoryService.GetItemForCreate();
            SetViewData();
            return View(result.Data);
        }

        // POST: Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryRequest categoryRequest)
        {
            var result = _categoryService.Validate(categoryRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Insert item logic:
                result = await _categoryService.Create(categoryRequest);
                
                if (result.Success)
                {
                    SetTempData(result.Message);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result.Message, result.HttpStatusCode);
            return View(categoryRequest);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Get item to edit logic:
            var result = await _categoryService.GetItemForEdit(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result.Data);
        }

        // POST: Categories/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryRequest categoryRequest)
        {
            var result = _categoryService.Validate(categoryRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Update item logic:
                result = await _categoryService.Update(categoryRequest);
                
                if (result.Success)
                {
                    SetTempData(result.Message);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result.Message, result.HttpStatusCode);
            return View(categoryRequest);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Get item to delete logic:
            var result = await _categoryService.GetItemForDelete(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result);
        }

        // POST: Categories/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(CategoryRequest categoryRequest)
        {
            // Delete item logic:
            var result = await _categoryService.Delete(categoryRequest);

            SetTempData(result.Message, result.HttpStatusCode);         
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(CategoryRequest categoryRequest)
        {
            // Delete item logic:
            var result = await _categoryService.Delete(categoryRequest);

            SetTempData(result.Message, result.HttpStatusCode);             
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFile(int id, string path = null)
        {
            // Delete file logic:
            var result = await _categoryService.DeleteFiles(id, path);

            SetTempData(result.Message, result.HttpStatusCode); 
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
                return File(result.Data.Stream, result.Data.ContentType, result.Data.Name);
            SetViewData(result.Message, result.HttpStatusCode);
            return View("_N4Cmessage");
        }
    }

    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategoryServiceApiController : ServiceApiController<Category, CategoryRequest, CategoryResponse>
    {
        public CategoryServiceApiController(Service<Category, CategoryRequest, CategoryResponse> categoryService) : base(categoryService)
        {
        }
    }
}
