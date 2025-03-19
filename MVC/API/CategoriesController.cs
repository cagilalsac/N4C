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
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Categories
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return ActionResult(await _mediator.Send(new CategoryQueryRequest()));
        }

        // GET: Categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return ActionResult(await _mediator.Send(new CategoryQueryRequest() { Id = id }), id);
        }

		// POST: Categories
        [HttpPost]
        public async Task<IActionResult> Post(CategoryCreateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // PUT: Categories
        [HttpPut]
        public async Task<IActionResult> Put(CategoryUpdateRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await _mediator.Send(request), request.Id);
        }

        // DELETE: Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await _mediator.Send(new CategoryDeleteRequest() { Id = id }), id);
        }
	}
}