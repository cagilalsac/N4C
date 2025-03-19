#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using N4C.Controllers;
using APP.Features.Products;

// Generated from N4C Template.

namespace MVC.API
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : ApiController
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Products
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            return ActionResult(await _mediator.Send(new ProductQueryRequest()));
        }

        // GET: Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return ActionResult(await _mediator.Send(new ProductQueryRequest() { Id = id }), id);
        }

		// POST: Products
        [HttpPost]
        public async Task<IActionResult> Post(ProductCreateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // PUT: Products
        [HttpPut]
        public async Task<IActionResult> Put(ProductUpdateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // DELETE: Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await _mediator.Send(new ProductDeleteRequest() { Id = id }), id);
        }



        #region For HTTP Requests from the HttpController
        // GET: Products/GetList
        [HttpGet("[action]"), AllowAnonymous]
        public async Task<IActionResult> GetList() => ActionResult(await _mediator.Send(new ProductQueryRequest()));

        // GET: Products/GetItem
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetItem(int id) => ActionResult(await _mediator.Send(new ProductQueryRequest() { Id = id }), id);
        #endregion
    }
}