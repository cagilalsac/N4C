#nullable disable
using Microsoft.AspNetCore.Mvc;
using MediatR;
using N4C.App;
using N4C.App.Features;
using APP.Features.Products;

// Generated from N4C Template.

namespace MVC.FeaturesApiControllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsApiController : ControllerBase
    {
        // Mediator injection:
        private readonly IMediator _mediator;

        public ProductsApiController (IMediator mediator)
        {
            _mediator = mediator;
        }

        ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // GET: ProductsApiController
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await _mediator.Send(new ProductQueryRequest());

            return ActionResult(result);
        }

        // GET: ProductsApiController/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await _mediator.Send(new ProductQueryRequest() { Id = id });
            
            return ActionResult(result);
        }

		// POST: ProductsApiController
        [HttpPost]
        public async Task<IActionResult> Post(ProductCommandRequest request)
        {
            request.Command = Commands.Create;
            request.ModelState = ModelState;

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: ProductsApiController
        [HttpPut]
        public async Task<IActionResult> Put(ProductCommandRequest request)
        {
            request.Command = Commands.Update;
            request.ModelState = ModelState;
            
            // Update item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // DELETE: ProductsApiController/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await _mediator.Send(new ProductCommandRequest() { Command = Commands.Delete, Id = id });

            return ActionResult(result);
        }
	}
}