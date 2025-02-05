#nullable disable
using Microsoft.AspNetCore.Mvc;
using MediatR;
using N4C.App;
using N4C.App.Features;
using APP.Features.Stores;

// Generated from N4C Template.

namespace MVC.FeaturesApiControllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StoresApiController : ControllerBase
    {
        // Mediator injection:
        private readonly IMediator _mediator;

        public StoresApiController (IMediator mediator)
        {
            _mediator = mediator;
        }

        ObjectResult ActionResult(Result result)
        {
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // GET: StoresApiController
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await _mediator.Send(new StoreQueryRequest());

            return ActionResult(result);
        }

        // GET: StoresApiController/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await _mediator.Send(new StoreQueryRequest() { Id = id });
            
            return ActionResult(result);
        }

		// POST: StoresApiController
        [HttpPost]
        public async Task<IActionResult> Post(StoreCommandRequest request)
        {
            request.Command = Commands.Create;
            request.ModelState = ModelState;

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: StoresApiController
        [HttpPut]
        public async Task<IActionResult> Put(StoreCommandRequest request)
        {
            request.Command = Commands.Update;
            request.ModelState = ModelState;
            
            // Update item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // DELETE: StoresApiController/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await _mediator.Send(new StoreCommandRequest() { Command = Commands.Delete, Id = id });

            return ActionResult(result);
        }
	}
}