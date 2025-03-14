using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.App
{
    public abstract class Request : Entity
    {
        [JsonIgnore]
        public ModelStateDictionary ModelState { get; private set; } = new ModelStateDictionary();

        public void Set(ModelStateDictionary modelState)
        {
            ModelState = modelState;
        }

        public void Add(string errorTR, string errorEN = default)
        {
            ModelState.AddModelError(string.Empty, errorTR + ";" + errorEN);
        }
    }

    public abstract class QueryRequest<TResponse> : Request, IRequest<Result<List<TResponse>>> where TResponse : Response, new()
    {
    }

    public abstract class CommandRequest : Request, IRequest<Result<List<Response>>>
    {
    }

    public abstract class CreateRequest : CommandRequest
    {
    }

    public abstract class UpdateRequest : CommandRequest
    {
    }

    public abstract class DeleteRequest : CommandRequest
    {
    }
}
