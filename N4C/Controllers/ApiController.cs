using Microsoft.AspNetCore.Mvc;
using N4C.Domain;
using N4C.Models;
using N4C.Services;

namespace N4C.Controllers
{
    public abstract class ApiController<TEntity, TRequest, TResponse> : ApiController 
        where TEntity : Entity, new() where TRequest : Request, new() where TResponse : Response, new()
    {
        protected override Service<TEntity, TRequest, TResponse> Service { get; }

        public ApiController(Service<TEntity, TRequest, TResponse> service) : base(service)
        {
            Service = service;
            Service.SetApi(true);
            Set();
        }

        protected override void Set(string culture = default, string titleTR = default, string titleEN = default)
        {
            base.Set(culture, titleTR, titleEN);
            Service.Set(Culture, titleTR, titleEN);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get(string pageNumber, string recordsPerPageCount, string orderExpression, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.GetResponse(pageNumber, recordsPerPageCount, orderExpression));
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.GetResponse(id));
        }

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] TRequest request, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.Create(request, ModelState));
        }

        [HttpPut]
        public virtual async Task<IActionResult> Put([FromBody] TRequest request, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.Update(request, ModelState));
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id, string culture = default)
        {
            Set(culture);
            return ActionResult(await Service.Delete(id));
        }
    }
}
