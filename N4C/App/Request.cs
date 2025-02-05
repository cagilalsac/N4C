using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.App.Features;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.App
{
    public abstract class Request : Entity
    {
    }

    public abstract class Request<TResponse> : Request where TResponse : Response, new()
    {
        [JsonIgnore]
        public Commands? Command { get; set; }
    }

    public abstract class QueryRequest<TResponse> : Request<TResponse>, IRequest<Result<List<TResponse>>> where TResponse : Response, new()
    {
    }

    public abstract class CommandRequest : Request<Response>, IRequest<Result<List<Response>>>
    {
        [JsonIgnore]
        public ModelStateDictionary ModelState { get; set; }
    }
}
