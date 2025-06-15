using Microsoft.AspNetCore.Mvc;
using N4C.Domain;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    public class ApiController<TEntity, TRequest, TResponse> : ApiController where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected Service<TEntity, TRequest, TResponse> Service { get; }

        public ApiController(Service<TEntity, TRequest, TResponse> service)
        {
            Service = service;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            return ActionResult(await Service.GetResponse());
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id)
        {
            return ActionResult(await Service.GetResponse(id));
        }

        [HttpPost]
        public virtual async Task<IActionResult> Post(TRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Create(request));
        }

        [HttpPut]
        public virtual async Task<IActionResult> Put(TRequest request)
        {
            request.Set(ModelState);
            return ActionResult(await Service.Update(request));
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            return ActionResult(await Service.Delete(id));
        }
    }
}
