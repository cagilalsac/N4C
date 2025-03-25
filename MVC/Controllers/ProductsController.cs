#nullable disable

using APP.Domain;
using APP.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using N4C.App;
using N4C.App.Services;
using N4C.Controllers;

// Generated from N4C Template.

namespace MVC.Controllers
{
    [Authorize]
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

        void SetViewData(Result result, PageOrder pageOrder = default)
        {
            // Related items logic to set ViewData SelectLists (Id and Name parameters may need to be changed in the SelectList constructors):
            
            ViewBag.CategoryId = new SelectList(_categoryService.GetList().Result.Data, "Id", "Name");

            /* Can be uncommented and used for many to many relationships. Entity must be replaced with the related name in the controller and views. */
            ViewBag.StoreIds = new MultiSelectList(_StoreService.GetList().Result.Data, "Id", "Name");

            SetViewData(_productService.Culture, result, _productService.Title, pageOrder);
        }

        // GET: Products
        [AllowAnonymous]
        public async Task<IActionResult> Index(PageOrder pageOrder)
        {
            // Get collection logic:
            var result = await _productService.GetList(pageOrder);
            
            SetViewData(result, pageOrder);
            return View(result);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Get item logic:
            var result = await _productService.GetItem(id);

            SetViewData(result);
            return View(result);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var result = _productService.GetItemForCreate();
            SetViewData(result);
            return View(result.Data);
        }

        // POST: Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRequest productRequest)
        {
            productRequest.Set(ModelState);

            // Insert item logic:
            var result = await _productService.Create(productRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(productRequest);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int id)
        {
            // Get item to edit logic:
            var result = _productService.GetItemForEdit(id);

            SetViewData(result);
            return View(result.Data);
        }

        // POST: Products/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductRequest productRequest)
        {
            productRequest.Set(ModelState);

            // Update item logic:
            var result = await _productService.Update(productRequest);
                
            if (result.Success)
            {
                SetTempData(result);
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });
            }
            
            SetViewData(result);
            return View(productRequest);
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int id)
        {
            // Get item to delete logic:
            var result = _productService.GetItemForDelete(id);

            SetViewData(result);
            return View(result);
        }

        // POST: Products/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProductRequest productRequest)
        {
            // Delete item logic:
            var result = await _productService.Delete(productRequest);

            SetTempData(result);         
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/DeleteByAlertify/5
        public async Task<IActionResult> DeleteByAlertify(ProductRequest productRequest)
        {
            // Delete item logic:
            var result = await _productService.Delete(productRequest);

            SetTempData(result);             
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteFile(int id, string path = null)
        {
            // Delete file logic:
            var result = await _productService.DeleteFiles(id, path);

            SetTempData(result); 
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
                return File(result.Data.FileStream, result.Data.FileContentType, result.Data.FileName);
            SetViewData(result);
            return View("_N4Cmessage");
        }
    }
}
