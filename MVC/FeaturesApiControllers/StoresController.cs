#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using N4C.Controllers;
using APP.Features.Stores;

// Generated from N4C Template.

namespace MVC.FeaturesApiControllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class StoresApiController : ApiController
    {
        // Mediator injection:
        private readonly IMediator _mediator;

        public StoresApiController (IMediator mediator)
        {
            _mediator = mediator;
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
        public async Task<IActionResult> Post(StoreCreateRequest request)
        {
            request.ModelState = ModelState;

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: StoresApiController
        [HttpPut]
        public async Task<IActionResult> Put(StoreUpdateRequest request)
        {
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
            var result = await _mediator.Send(new StoreDeleteRequest() { Id = id });

            return ActionResult(result);
        }
	}
}