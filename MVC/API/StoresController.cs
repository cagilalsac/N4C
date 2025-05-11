#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using N4C.Controllers;
using APP.Features.Stores;

// Generated from N4C Template.

namespace MVC.API
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class StoresController : ApiController
    {
        private readonly IMediator _mediator;

        public StoresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Stores
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return ActionResult(await _mediator.Send(new StoreQueryRequest()));
        }

        // GET: Stores/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return ActionResult(await _mediator.Send(new StoreQueryRequest() { Id = id }), id);
        }

		// POST: Stores
        [HttpPost]
        public async Task<IActionResult> Post(StoreCreateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // PUT: Stores
        [HttpPut]
        public async Task<IActionResult> Put(StoreUpdateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // DELETE: Stores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await _mediator.Send(new StoreDeleteRequest() { Id = id }), id);
        }
	}
}