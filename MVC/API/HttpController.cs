using Microsoft.AspNetCore.Mvc;
using MVC.API.Models;
using N4C.App.Services;
using N4C.Controllers;

namespace MVC.API
{
    public class HttpController : ApiController
    {
        private readonly HttpService _httpService;

        public HttpController(HttpService httpService)
        {
            _httpService = httpService;
            _httpService.Set(c => c.ApiUri = "products");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductListFromResult()
        {
            return ActionResult(await _httpService.Get<ProductHttpResponse>());
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductList()
        {
            _httpService.Set(c => c.ApiUri = "products/getlist");
            return ActionResult(await _httpService.Get<ProductHttpResponse>());
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetProductItemFromResult(int id)
        {
            return ActionResult(await _httpService.Get<ProductHttpResponse>(id));
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetProductItem(int id)
        {
            _httpService.Set(c => c.ApiUri = "products/getitem");
            return ActionResult(await _httpService.Get<ProductHttpResponse>(id));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PostProductItem(ProductHttpRequest request)
        {
            return ActionResult(await _httpService.Create(request));
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> PutProductItem(ProductHttpRequest request)
        {
            return ActionResult(await _httpService.Update(request));
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteProductItem(int id)
        {
            return ActionResult(await _httpService.Delete(id));
        }
    }
}
