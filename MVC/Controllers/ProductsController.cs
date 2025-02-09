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
    public class ProductsController : MvcController
    {
        // Service injections:
        private readonly Service<Product, ProductRequest, ProductResponse> _productService;
        private readonly Service<Category, CategoryRequest, CategoryResponse> _categoryService;

        /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
        private readonly Service<Store, StoreRequest, StoreResponse> _StoreService;

        public ProductsController(Service<Product, ProductRequest, ProductResponse> productService
            , Service<Category, CategoryRequest, CategoryResponse> categoryService

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            , Service<Store, StoreRequest, StoreResponse> StoreService
        )
        {
            _productService = productService;
            _categoryService = categoryService;

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            _StoreService = StoreService;
        }

        void SetViewData(string message = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):
            
            ViewBag.CategoryId = new SelectList(_categoryService.GetList().Result.Data, "Id", "Name");

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            ViewBag.StoreIds = new MultiSelectList(_StoreService.GetList().Result.Data, "Id", "Name");

            SetViewData(_productService.Culture, message, httpStatusCode, _productService.Title, pageOrder);
        }

        // GET: Products
        //[AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = pageOrder is null ? await _productService.GetList() : await _productService.GetList(pageOrder);
            
            SetViewData(result.Message, result.HttpStatusCode, pageOrder);
            return View(result);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _productService.GetItem(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var result = _productService.GetItemForCreate();
            SetViewData();
            return View(result.Data);
        }

        // POST: Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRequest productRequest)
        {
            var result = _productService.Validate(productRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Insert item logic:
                result = await _productService.Create(productRequest);
                
                if (result.Success)
                {
                    SetTempData(result.Message);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result.Message, result.HttpStatusCode);
            return View(productRequest);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Get item to edit logic:
            var result = await _productService.GetItemForEdit(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result.Data);
        }

        // POST: Products/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductRequest productRequest)
        {
            var result = _productService.Validate(productRequest, ModelState, "<br>");
            if (result.Success)
            {
                // Update item logic:
                result = await _productService.Update(productRequest);
                
                if (result.Success)
                {
                    SetTempData(result.Message);
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
            }
            SetViewData(result.Message, result.HttpStatusCode);
            return View(productRequest);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Get item to delete logic:
            var result = await _productService.GetItemForDelete(id);

            SetViewData(result.Message, result.HttpStatusCode);
            return View(result);
        }

        // POST: Products/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProductRequest productRequest)
        {
            // Delete item logic:
            var result = await _productService.Delete(productRequest);

            SetTempData(result.Message, result.HttpStatusCode);         
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(ProductRequest productRequest)
        {
            // Delete item logic:
            var result = await _productService.Delete(productRequest);

            SetTempData(result.Message, result.HttpStatusCode);             
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFile(int id, string path = null)
        {
            // Delete file logic:
            var result = await _productService.DeleteFiles(id, path);

            SetTempData(result.Message, result.HttpStatusCode); 
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task ExportToExcel()
        {
            await _productService.GetExcel();
        }

        public IActionResult Download(string path)
        {
            var result = _productService.GetFile(path);
            if (result.Success)
                return File(result.Data.Stream, result.Data.ContentType, result.Data.Name);
            SetViewData(result.Message, result.HttpStatusCode);
            return View("_N4Cmessage");
        }
    }

    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductServiceApiController : ServiceApiController<Product, ProductRequest, ProductResponse>
    {
        public ProductServiceApiController(Service<Product, ProductRequest, ProductResponse> productService) : base(productService)
        {
        }
    }
}
