#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using N4C.Controllers;
using APP.Features.Categories;

// Generated from N4C Template.

namespace MVC.API
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class CategoriesController : ApiController
    {
        // Mediator injection:
        private readonly IMediator _mediator;

        public CategoriesController (IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Categories
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await _mediator.Send(new CategoryQueryRequest());

            return ActionResult(result);
        }

        // GET: Categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await _mediator.Send(new CategoryQueryRequest() { Id = id });
            
            return ActionResult(result);
        }

		// POST: Categories
        [HttpPost]
        public async Task<IActionResult> Post(CategoryCreateRequest request)
        {
            request.Set(ModelState);

            // Insert item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // PUT: Categories
        [HttpPut]
        public async Task<IActionResult> Put(CategoryUpdateRequest request)
        {
            request.Set(ModelState);

            // Update item logic:
            var result = await _mediator.Send(request);

            return ActionResult(result);
        }

        // DELETE: Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await _mediator.Send(new CategoryDeleteRequest() { Id = id });

            return ActionResult(result);
        }
	}
}