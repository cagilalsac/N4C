using Microsoft.AspNetCore.Mvc;
using N4C.App.Services;
using N4C.App;
using N4C.Domain;

namespace N4C.Controllers
{
    public abstract class ServiceApiController<TEntity, TRequest, TResponse> : ApiController
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        // Service injection:
        protected Service<TEntity, TRequest, TResponse> Service;

        protected ServiceApiController(Service<TEntity, TRequest, TResponse> service)
        {
            Service = service;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            // Get collection logic:
            var result = await Service.GetList();

            return ActionResult(result);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id)
        {
            // Get item logic:
            var result = await Service.GetItem(id);

            return ActionResult(result);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Post(TRequest request)
        {
            var result = Service.Validate(request, ModelState);
            if (result.Success)
            {
                // Insert item logic:
                result = await Service.Create(request);
            }
            return ActionResult(result);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Put(TRequest request)
        {
            var result = Service.Validate(request, ModelState);
            if (result.Success)
            {
                // Update item logic:
                result = await Service.Update(request);
            }
            return ActionResult(result);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            // Delete item logic:
            var result = await Service.Delete(new TRequest() { Id = id });

            return ActionResult(result);
        }
    }
}
