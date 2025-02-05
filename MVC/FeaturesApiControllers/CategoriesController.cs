#nullable disable
using Microsoft.AspNetCore.Mvc;
using MediatR;
using N4C.App;
using N4C.App.Features;
using APP.Features.Categories;

// Generated from N4C Template.

namespace MVC.FeaturesApiControllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategoriesApiController : ControllerBase
    {
        // Mediator injection:
        private readonly IMediator _mediator;

        public CategoriesApiController (IMediator mediator)
        {
            _mediator = mediator;
        }

        ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // GET: CategoriesApiController
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await _mediator.Send(new CategoryQueryRequest());

            return ActionResult(result);
        }

        // GET: CategoriesApiController/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await _mediator.Send(new CategoryQueryRequest() { Id = id });
            
            return ActionResult(result);
        }

		// POST: CategoriesApiController
        [HttpPost]
        public async Task<IActionResult> Post(CategoryCommandRequest request)
        {
            request.Command = Commands.Create;
            request.ModelState = ModelState;

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: CategoriesApiController
        [HttpPut]
        public async Task<IActionResult> Put(CategoryCommandRequest request)
        {
            request.Command = Commands.Update;
            request.ModelState = ModelState;
            
            // Update item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // DELETE: CategoriesApiController/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await _mediator.Send(new CategoryCommandRequest() { Command = Commands.Delete, Id = id });

            return ActionResult(result);
        }
	}
}