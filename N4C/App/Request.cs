using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.App
{
    public abstract class Request : Entity
    {
    }

    public abstract class QueryRequest<TResponse> : Request, IRequest<Result<List<TResponse>>> where TResponse : Response, new()
    {
    }

    public abstract class CommandRequest : Request, IRequest<Result<List<Response>>>
    {
    }

    public abstract class CreateRequest : CommandRequest
    {
        [JsonIgnore]
        public ModelStateDictionary ModelState { get; set; }
    }

    public abstract class UpdateRequest : CommandRequest
    {
        [JsonIgnore]
        public ModelStateDictionary ModelState { get; set; }
    }

    public abstract class DeleteRequest : CommandRequest
    {
    }
}
