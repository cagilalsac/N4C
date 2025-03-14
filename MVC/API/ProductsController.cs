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
        // Mediator injection:
        private readonly IMediator _mediator;

        public ProductsController (IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Products
        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await _mediator.Send(new ProductQueryRequest());

            return ActionResult(result);
        }

        // GET: Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await _mediator.Send(new ProductQueryRequest() { Id = id });
            
            return ActionResult(result);
        }

		// POST: Products
        [HttpPost]
        public async Task<IActionResult> Post(ProductCreateRequest request)
        {
            request.Set(ModelState);

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: Products
        [HttpPut]
        public async Task<IActionResult> Put(ProductUpdateRequest request)
        {
            request.Set(ModelState);

            // Update item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // DELETE: Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await _mediator.Send(new ProductDeleteRequest() { Id = id });

            return ActionResult(result);
        }
	}
}