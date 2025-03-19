using Microsoft.AspNetCore.Mvc;
using MVC.API.Models;
using N4C.App.Services;
using N4C.Controllers;

namespace MVC.API
{
    public class HttpController : ApiController
    {
        const string PRODUCTSURI = "https://localhost:7008/api/products";

        private readonly HttpService _httpService;

        public HttpController(HttpService httpService)
        {
            _httpService = httpService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductListFromResult()
        {
            return ActionResult(await _httpService.GetList<ProductHttpResponse>(PRODUCTSURI));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductList()
        {
            return ActionResult(await _httpService.GetList<ProductHttpResponse>($"{PRODUCTSURI}/GetList"));
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetProductItemFromResult(int id)
        {
            return ActionResult(await _httpService.GetItem<ProductHttpResponse>($"{PRODUCTSURI}", id));
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetProductItem(int id)
        {
            return ActionResult(await _httpService.GetItem<ProductHttpResponse>($"{PRODUCTSURI}/GetItem", id));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PostProductItem(ProductHttpRequest request)
        {
            return ActionResult(await _httpService.Create($"{PRODUCTSURI}", request));
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> PutProductItem(ProductHttpRequest request)
        {
            return ActionResult(await _httpService.Update($"{PRODUCTSURI}", request));
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteProductItem(int id)
        {
            return ActionResult(await _httpService.Delete($"{PRODUCTSURI}", id));
        }
    }
}
